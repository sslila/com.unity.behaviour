using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;

namespace Unity.Behavior
{
    /// <summary>
    /// Dummy type to represent regular text in a sentence
    /// </summary>
    internal class RegularText
    {
    }

    internal class WordTypePair
    {
        internal string Word { get; } 
        internal Type Type { get; set; }

        internal WordTypePair([NotNull] string word, [NotNull] Type type)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException($"Initialization argument {nameof(word)} must not be null or empty!");
            }

            if (type == null)
            {
                throw new ArgumentException($"Initialization argument {nameof(type)} must not be null!");
            }
            
            Word = word;
            Type = type;
        }
    }
    
    internal class WordTypeSentence
    {
        internal string LastSentence = string.Empty;

        private Dictionary<string, Type> m_VariableSuggestions;
        
        internal IEnumerable<WordTypePair> WordTypeParameters =>
            WordTypePairs.Where(p => p.Type != typeof(RegularText));

        internal List<WordTypePair> WordTypePairs { get; set; } = new ();
        private readonly Dictionary<string, Type> m_WordTypeCache = new ();

        public override string ToString() => 
            string.Join(" ", WordTypePairs.Select(p => p.Type == typeof(RegularText) ? p.Word : $"[{p.Word}]"));
        
        internal void SetWordType(int index, string word, Type type)
        {
            WordTypePairs[index] = new WordTypePair(word, type);
            m_WordTypeCache[word] = type;
        }

        internal void AddWordType(string word, Type type)
        {
            WordTypePairs.Add(new WordTypePair(word, type));
            m_WordTypeCache[word] = type;
        }
        
        internal Dictionary<string, Type> GetStoryVariables()
        {
#if UNITY_EDITOR
            return WordTypeParameters.ToDictionary(p => ObjectNames.NicifyVariableName(p.Word), p => p.Type);
#else
            return WordTypeParameters.ToDictionary(p => Util.NicifyVariableName(p.Word), p => p.Type);
#endif
        }
        

        internal void UpdateWordTypeList(int cursorIndex, string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                WordTypePairs.Clear();
                return;
            }
            var words = ToWordArray(sentence);
            List<WordTypePair> newPairs = words.Select(word => new WordTypePair(word, typeof(RegularText))).ToList();

            if (WordTypePairs.Count == 0 && !string.IsNullOrEmpty(sentence))
            {
                WordTypePairs = newPairs;
            }
            
            for (var i = 0; i < WordTypePairs.Count && i < newPairs.Count; i++)
            {
                var currentPair = WordTypePairs[i];
                var newPair = newPairs[i];

                // If after the description changed the word is the same, default to the same type as in the old pair list.
                if (currentPair.Word == newPair.Word)
                {
                    newPair.Type = currentPair.Type;
                }
                // Check if variable suggestions from the asset's Blackboard contains the word.
                if (m_VariableSuggestions != null && m_VariableSuggestions.TryGetValue(newPair.Word.ToLower(), out Type suggestion))
                {
                    bool cacheContainsNewPair = m_WordTypeCache.TryGetValue(newPair.Word, out Type type) && type == newPair.Type;
                    if (!cacheContainsNewPair)
                    {
                        newPair.Type = suggestion;      
                    }
                }
                else if (WordTypePairs.Count == newPairs.Count)
                {
                    // Check if the word has been stored earlier with a specific type and is a unique pair
                    var isKnownAndUniqueWord = m_WordTypeCache.TryGetValue(newPair.Word, out var type) &&
                                               PairIsUnique(newPair.Word, type);
            
                    newPair.Type = isKnownAndUniqueWord ? type : currentPair.Type;
                }
                else if (newPairs.Count > WordTypePairs.Count)
                {
                    // Words were inserted. Look forward up to the difference between the lists and see if the word can be found there.
                    if (currentPair.Word != newPair.Word)
                    {
                        var difference = newPairs.Count - WordTypePairs.Count;
                        for (int j = i+1; j <= i+difference; j++)
                        {
                            var newWordTypePair = newPairs[j];
                            if (newWordTypePair.Word == currentPair.Word)
                            {
                                newWordTypePair.Type = currentPair.Type;
                                newPairs[j] = newWordTypePair;
                            }
                        }
                    }
                    // Check for a case when words are added adjacent to the same words in the sentence
                    else if (!IsCursorAtLastWordsEnd(cursorIndex, sentence))
                    {
                        var difference = newPairs.Count - WordTypePairs.Count;
                        var wordIndex = CursorIndexToWordIndex(cursorIndex, sentence);
                        if (i >= wordIndex)
                        {
                            for (int j = wordIndex; j <= i+difference && j < newPairs.Count; j++)
                            {
                                var newWordTypePair = newPairs[j];
                                if (newWordTypePair.Word == WordTypePairs[j-difference].Word)
                                {
                                    newWordTypePair.Type = WordTypePairs[j-difference].Type;
                                    newPairs[j] = newWordTypePair;
                                }
                            }   
                        }
                    }
                }
                else if (newPairs.Count < WordTypePairs.Count)
                {
                    // Words were removed. Look up to the difference between the lists and see if the word can be found there.
                    if (currentPair.Word != newPair.Word)
                    {
                        var difference = WordTypePairs.Count - newPairs.Count;
                        for (int j = i+1; j < j+difference && j < WordTypePairs.Count; j++)
                        {
                            if (newPair.Word == WordTypePairs[j].Word)
                            {
                                newPair.Type = WordTypePairs[j].Type;
                            }
                        }
                    }
                    // Check for a case when adjacent words with the same Word are removed from the sentence
                    else if (!IsCursorAtLastWordsEnd(cursorIndex, sentence))
                    {
                        var difference = WordTypePairs.Count - newPairs.Count;
                        var removedWordIndex = CursorIndexToWordIndex(cursorIndex, LastSentence);
                        for (int j = removedWordIndex+difference; j < WordTypePairs.Count; j++)
                        {
                            if (i >= removedWordIndex && newPair.Word == WordTypePairs[j].Word)
                            {
                                newPair.Type = WordTypePairs[j].Type; 
                            }
                        }
                    }
                }
                newPairs[i] = newPair;
            }
            WordTypePairs = newPairs;
            LastSentence = sentence;
        }
        
        private static bool IsCursorAtLastWordsEnd(int cursorIndex, string sentence)
        {
            if (sentence == null)
            {
                return false;
            }
            return cursorIndex == sentence.TrimEnd().Length;
        }
        
        private static int CursorIndexToWordIndex(int cursorIndex, string sentence)
        {
            return sentence == null ? 0 : ToWordArray(sentence[0..cursorIndex]).Length;
        }
        
        private static string[] ToWordArray(string text)
        {
            return text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        
        private bool PairIsUnique(string word, Type type)
        {
#if UNITY_EDITOR            
            var pair = new WordTypePair(ObjectNames.NicifyVariableName(word), type);
            return WordTypePairs.All(wordTypePair => pair.Word != ObjectNames.NicifyVariableName(wordTypePair.Word) || pair.Type != wordTypePair.Type);
#else
            var pair = new WordTypePair(Util.NicifyVariableName(word), type);
            return WordTypePairs.All(wordTypePair => pair.Word != Util.NicifyVariableName(wordTypePair.Word) || pair.Type != wordTypePair.Type);
#endif
        }

        internal void AddSuggestions(Dictionary<string, Type> variableSuggestions)
        {
            m_VariableSuggestions = variableSuggestions;
        }
        
    }
}