#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity.Behavior
{
    internal static class GeneratorUtils
    {
        internal static string GenerateVariableFields(Dictionary<string, Type> variables)
        {
            StringBuilder functionStringBuilder = new StringBuilder();
            using (StringWriter outfile = new StringWriter(functionStringBuilder))
            {
                foreach (KeyValuePair<string, Type> item in variables)
                {
                    string typeOfObject = GetStringForType(item.Value);
                    // Todo: Improve this placeholder check for generating operator field attributes.
                    if (item.Value == typeof(ConditionOperator) || item.Value == typeof(BooleanOperator))
                    {
                        typeOfObject = "ConditionOperator";
                        string comparisonType = "All";
                        if (item.Value == typeof(BooleanOperator))
                        {
                            comparisonType = "Boolean";
                        }
                        outfile.WriteLine($"    [Comparison(comparisonType: ComparisonType.{comparisonType})]");
                    }
                    outfile.WriteLine(
                        $"    [SerializeReference] public BlackboardVariable<{typeOfObject}> {RemoveSpaces(NicifyString(item.Key))};");
                }
            }

            return functionStringBuilder.ToString();
        }

        internal static string GetStringForType(Type type)
        {
            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(bool))
            {
                return "bool";
            }

            if (type == typeof(string))
            {
                return "string";
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                return $"List<{GetStringForType(type.GetGenericArguments().First())}>";
            }
          
            return type.Name;
        }

        internal static string NicifyString(string str)
        {
#if UNITY_EDITOR
            return UnityEditor.ObjectNames.NicifyVariableName(str);
#else
            return Util.NicifyVariableName(str);
#endif
        }
        
        
        internal static string ToPascalCase(string text)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(text, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }

        internal static string RemoveSpaces(string str)
        {
            return str == null ? string.Empty : str.Replace(" ", string.Empty);
        }
        
        static string GetBlackboardToVariableString(string name, Type type)
        {
            return $"{RemoveSpaces(NicifyString(name))} = BlackboardVariable.GetTypedBlackboardVariableValue<{GetStringForType(type)}>(var);";
        }

        static string GetVariableToBlackboardString(string name, Type type)
        {
            return $"BlackboardVariable.SetTypedBlackboardVariableValue<{GetStringForType(type)}>(var, {RemoveSpaces(NicifyString(name))});";
        }
    }
}
#endif