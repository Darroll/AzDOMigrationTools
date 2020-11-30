using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO.Collections
{
    /// <summary>
    /// Types of abstract identifier that can be created.
    /// </summary>
    public enum AbstractIdentifierType
    {
        NumericalIdentifier = 0,
        AlphabeticalIdentifier
    }

    /// <summary>
    /// Class to store data fields and generate an abstracted identifier using
    /// one or many of these fields as composition rules.
    /// </summary>
    public class AbstractIdentification
    {
        #region - Private Members.

        private readonly Dictionary<string, int> _abstractIdentifiersTable = null;
        private readonly Dictionary<int, string[]> _internalStorage = null;
        private readonly AbstractIdentifierType _identifierType = AbstractIdentifierType.NumericalIdentifier;
        private int _storageKey = 0;

        #region - Methods.

        /// <summary>
        /// Add n value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="valueFields">Array of value fields stored as strings</param>
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        private string[] AddValueFieldsInternal(string[] valueFields, string indexes)
        {
            // Initialize.
            int genericIdentifier = 0;
            int asciiValue;
            int i;
            int index;
            int nbOfValues = valueFields.Length;
            string alphaIdentifier;
            string key;
            List<string> lvs = new List<string>();
            List<string> storageRecordNonNullValues = new List<string>();

            // Size storage record based on the number of values + 1 abstract identifier.
            string[] storageRecord = new string[nbOfValues + 1];

            // keys parameters is a comma-separated list of index that indicate which values are used to form the key.
            foreach (string fieldIndex in indexes.Split(new char[] { ',' }))
            {
                // Convert into int. for manipulation.
                index = Convert.ToInt32(fieldIndex);

                // Check if index is out of bound.
                if(index > nbOfValues - 1)
                    throw (new ArgumentException("Only index 0 to the length of indexes - 1 can be used with indexes parameter."));
                else
                    lvs.Add(valueFields[index]);
            }

            // Concatenate all elements to form a unique key. Each element is separated by a comma.
            key = string.Join(",", lvs.ToArray());

            // Generate the abstract identifier.
            if (_abstractIdentifiersTable.ContainsKey(key))
            {
                // Get the latest identifier and increment.
                genericIdentifier = _abstractIdentifiersTable[key];
                genericIdentifier++;
                _abstractIdentifiersTable[key] = genericIdentifier;
            }
            else
            {
                // Increment.
                genericIdentifier++;

                // Store identifier.
                _abstractIdentifiersTable.Add(key, genericIdentifier);
            }

            if (_identifierType == AbstractIdentifierType.NumericalIdentifier)
            {
                // Reset generic counter.
                i = 0;

                // Store all values
                foreach (string v in valueFields)
                { 
                    storageRecord[i] = v;
                    i++;
                }

                // Store generic identifier
                storageRecord[i] = genericIdentifier.ToString();
            }
            else if (_identifierType == AbstractIdentifierType.AlphabeticalIdentifier)
            {
                // First char. starts at ASCII 65 (A) and ends at 122 (Z).                
                if (genericIdentifier <= 26)
                {
                    asciiValue = genericIdentifier + 64;
                    alphaIdentifier = Convert.ToChar(asciiValue).ToString();
                }
                else
                {
                    // This will generate a double letter identifier.
                    var v = genericIdentifier;
                    var b = 0;

                    while (true)
                    {
                        if (v - 26 < 0)
                        {
                            // Set first letter of double letter identification.
                            asciiValue = b + 64;
                            alphaIdentifier = Convert.ToChar(asciiValue).ToString();

                            // Set second letter of double letter identification.
                            asciiValue = v + 64;
                            alphaIdentifier += Convert.ToChar(asciiValue).ToString();

                            // Leave the loop.
                            break;
                        }
                        else
                        {
                            // Decrement by 26.
                            v -= 26;

                            // Increment the number of 26 char. block you have.
                            b++;
                        }
                    }
                }

                // Reset generic counter.
                i = 0;

                // Store all values
                foreach (string v in valueFields)
                {
                    storageRecord[i] = v;
                    i++;
                }

                // Store generic identifier
                storageRecord[i] = alphaIdentifier;
            }

            // Store internally.
            _storageKey++;
            _internalStorage.Add(_storageKey, storageRecord);

            // Generate array of string values only if non null.
            foreach (string v in storageRecord)
                if (!string.IsNullOrEmpty(v))
                    storageRecordNonNullValues.Add(v);

            // Return the storage record without the null values.
            return storageRecordNonNullValues.ToArray();
        }

        #endregion

        #endregion

        #region - Public Members.

        #region - Properties.

        /// <summary>
        /// Return the number of storage records.
        /// </summary>
        public int Count
        {
            get
            {
                return _internalStorage.Count;
            }
        }

        /// <summary>
        /// Return all non-null and non-empty value fields stored.
        /// </summary>
        public List<string[]> ValueFields {
            get
            {
                // Initialize.
                List<string> storageRecordNonNullValues = null;
                List<string[]> lvs = new List<string[]>();

                // Browse internal storage in order of the key.
                foreach (var item in _internalStorage.OrderBy(x => x.Key))
                {
                    // Instantiate.
                    storageRecordNonNullValues = new List<string>();

                    // Generate array of string values only if non null.
                    foreach (string v in item.Value)
                        if (!string.IsNullOrEmpty(v))
                            storageRecordNonNullValues.Add(v);

                    // Add to full list of vlaues.
                    lvs.Add(storageRecordNonNullValues.ToArray());
                }

                // Return the full list of values.
                return lvs;
            }
        }

        #endregion

        #region - Constructors.

        /// <summary>
        /// Default constructor that will use by default a numeral identifier.
        /// </summary>
        public AbstractIdentification()
        {
            // By default, it will create a numeral identification.
            _identifierType = AbstractIdentifierType.NumericalIdentifier;

            // Instantiate.
            _abstractIdentifiersTable = new Dictionary<string, int>();
            _internalStorage = new Dictionary<int, string[]>();
        }

        /// <summary>
        /// Constructor that let you choose which type of identifier to use.
        /// </summary>
        public AbstractIdentification(AbstractIdentifierType identifierType)
        {
            // By default, it will create a numeral identification.
            _identifierType = identifierType;

            // Instantiate.
            _abstractIdentifiersTable = new Dictionary<string, int>();
            _internalStorage = new Dictionary<int, string[]>();
        }

        #endregion

        #region - Methods.

        /// <summary>
        /// Add 2 value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="value1">Value field stored as string</param>
        /// <param name="value2">Value field stored as string</param>        
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        public string[] AddValueFields(string value1, string value2, string indexes)
        {
            string[] values = new string[] { value1, value2};
            return AddValueFieldsInternal(values, indexes);
        }

        /// <summary>
        /// Add 3 value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="value1">Value field stored as string</param>
        /// <param name="value2">Value field stored as string</param>
        /// <param name="value3">Value field stored as string</param>        
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        public string[] AddValueFields(string value1, string value2, string value3, string indexes)
        {
            string[] values = new string[] { value1, value2, value3};
            return AddValueFieldsInternal(values, indexes);
        }

        /// <summary>
        /// Add 4 value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="value1">Value field stored as string</param>
        /// <param name="value2">Value field stored as string</param>
        /// <param name="value3">Value field stored as string</param>
        /// <param name="value4">Value field stored as string</param>
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        public string[] AddValueFields(string value1, string value2, string value3, string value4, string indexes)
        {
            string[] values = new string[] { value1, value2, value3, value4 };
            return AddValueFieldsInternal(values, indexes);
        }

        /// <summary>
        /// Add 5 value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="value1">Value field stored as string</param>
        /// <param name="value2">Value field stored as string</param>
        /// <param name="value3">Value field stored as string</param>
        /// <param name="value4">Value field stored as string</param>
        /// <param name="value5">Value field stored as string</param>
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        public string[] AddValueFields(string value1, string value2, string value3, string value4, string value5, string indexes)
        {
            string[] values = new string[] { value1, value2, value3, value4, value5 };
            return AddValueFieldsInternal(values, indexes);
        }

        /// <summary>
        /// Add 6 value fields to store and indicate which fields will be used for composition rules
        /// to generate an abstracted identifier.        
        /// </summary>        
        /// <param name="value1">Value field stored as string</param>
        /// <param name="value2">Value field stored as string</param>
        /// <param name="value3">Value field stored as string</param>
        /// <param name="value4">Value field stored as string</param>
        /// <param name="value5">Value field stored as string</param>
        /// <param name="value6">Value field stored as string</param>
        /// <param name="indexes">Comma-separated list of indexes of the value fields to use for composition rules. Start with 0 for first item, 1 for second item, etc.</param>
        /// <returns>Array of strings</returns>
        public string[] AddValueFields(string value1, string value2, string value3, string value4, string value5, string value6, string indexes)
        {
            string[] values = new string[] { value1, value2, value3, value4, value5, value6 };
            return AddValueFieldsInternal(values, indexes);
        }

        #endregion

        #endregion
    }
}