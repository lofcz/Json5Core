using System;
using System.Collections.Generic;

namespace Json5Core;

public sealed class Json5Parameters
{
    /// <summary>
    /// Use the optimized fast Dataset Schema format (default = True)
    /// </summary>
    public bool UseOptimizedDatasetSchema = true;

    /// <summary>
    /// Use the fast GUID format (default = True)
    /// </summary>
    public bool UseFastGuid = true;

    /// <summary>
    /// Serialize null values to the output (default = True)
    /// </summary>
    public bool SerializeNullValues = true;

    /// <summary>
    /// Use the UTC date format (default = True)
    /// </summary>
    public bool UseUTCDateTime = true;

    /// <summary>
    /// Show the readonly properties of types in the output (default = False)
    /// </summary>
    public bool ShowReadOnlyProperties;

    /// <summary>
    /// Use the $types extension to optimise the output json (default = True)
    /// </summary>
    public bool UsingGlobalTypes = true;

    /// <summary>
    /// Ignore case when processing json and deserializing 
    /// </summary>
    [Obsolete("Not needed anymore and will always match")]
    public bool IgnoreCaseOnDeserialize = false;

    /// <summary>
    /// Anonymous types have read only properties 
    /// </summary>
    public bool EnableAnonymousTypes;

    /// <summary>
    /// Enable Json5Core extensions $types, $type, $map (default = False)
    /// </summary>
    public bool UseExtensions;

    /// <summary>
    /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
    /// </summary>
    public bool UseEscapedUnicode = true;

    /// <summary>
    /// Output string key dictionaries as "k"/"v" format (default = False) 
    /// </summary>
    public bool KVStyleStringDictionary;

    /// <summary>
    /// Output Enum values instead of names (default = False)
    /// </summary>
    public bool UseValuesOfEnums;

    /// <summary>
    /// Ignore attributes to check for (default : XmlIgnoreAttribute, NonSerialized)
    /// </summary>
    public List<Type> IgnoreAttributes = [typeof(System.Xml.Serialization.XmlIgnoreAttribute), typeof(NonSerializedAttribute)];

    /// <summary>
    /// If you have parametric and no default constructor for you classes (default = False)
    /// 
    /// IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
    /// </summary>
    public bool ParametricConstructorOverride;

    /// <summary>
    /// Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
    /// </summary>
    public bool DateTimeMilliseconds;

    /// <summary>
    /// Maximum depth for circular references in inline mode (default = 20)
    /// </summary>
    public byte SerializerMaxDepth = 20;

    /// <summary>
    /// Inline circular or already seen objects instead of replacement with $i (default = false) 
    /// </summary>
    public bool InlineCircularReferences;

    /// <summary>
    /// Save property/field names as lowercase (default = false)
    /// </summary>
    public bool SerializeToLowerCaseNames;

    /// <summary>
    /// Formatter indent spaces (default = 3)
    /// </summary>
    public byte FormatterIndentSpaces = 3;

    /// <summary>
    /// Auto convert string values to numbers when needed (default = true)
    /// 
    /// When disabled you will get an exception if the types don't match
    /// </summary>
    public bool AutoConvertStringToNumbers = true;

    /// <summary>
    /// Override object equality hash code checking (default = false)
    /// </summary>
    public bool OverrideObjectHashCodeChecking;

    /// <summary>
    /// Checking list of bad types to prevent friday 13th json attacks (default = true)
    /// 
    /// Will throw an exception if encountered and set
    /// </summary>
    public bool BadListTypeChecking = true;

    /// <summary>
    /// Fully Qualify the DataSet Schema (default = false)
    /// 
    /// If you get deserialize errors with DataSets and DataTables
    /// </summary>
    public bool FullyQualifiedDataSetSchema;

    ///// <summary>
    ///// PENDING : Allow json5 (default = false)
    ///// </summary>
    //public bool AllowJsonFive = false;

    public void FixValues()
    {
        if (!UseExtensions) // disable conflicting params
        {
            UsingGlobalTypes = false;
            InlineCircularReferences = true;
        }

        if (EnableAnonymousTypes)
        {
            ShowReadOnlyProperties = true;
        }

        //if (AllowJsonFive)
        //    AllowNonQuotedKeys = true;
    }

    public Json5Parameters MakeCopy()
    {
        return new Json5Parameters
        {
            DateTimeMilliseconds = DateTimeMilliseconds,
            EnableAnonymousTypes = EnableAnonymousTypes,
            FormatterIndentSpaces = FormatterIndentSpaces,
            IgnoreAttributes = [..IgnoreAttributes],
            //IgnoreCaseOnDeserialize = IgnoreCaseOnDeserialize,
            InlineCircularReferences = InlineCircularReferences,
            KVStyleStringDictionary = KVStyleStringDictionary,
            ParametricConstructorOverride = ParametricConstructorOverride,
            SerializeNullValues = SerializeNullValues,
            SerializerMaxDepth = SerializerMaxDepth,
            SerializeToLowerCaseNames = SerializeToLowerCaseNames,
            ShowReadOnlyProperties = ShowReadOnlyProperties,
            UseEscapedUnicode = UseEscapedUnicode,
            UseExtensions = UseExtensions,
            UseFastGuid = UseFastGuid,
            UseOptimizedDatasetSchema = UseOptimizedDatasetSchema,
            UseUTCDateTime = UseUTCDateTime,
            UseValuesOfEnums = UseValuesOfEnums,
            UsingGlobalTypes = UsingGlobalTypes,
            AutoConvertStringToNumbers = AutoConvertStringToNumbers,
            OverrideObjectHashCodeChecking = OverrideObjectHashCodeChecking,
            //BlackListTypeChecking = BlackListTypeChecking,
            FullyQualifiedDataSetSchema = FullyQualifiedDataSetSchema,
            BadListTypeChecking = BadListTypeChecking,
            //AllowJsonFive = AllowJsonFive
        };
    }
}