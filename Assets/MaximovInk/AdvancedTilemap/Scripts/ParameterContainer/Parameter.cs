namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class Parameter
    {
        public string name;
        public bool isHidden;

        public ParameterType type;

        public bool boolValue;
        public int intValue;
        public float floatValue;
        public UnityEngine.Object objectValue;
        public string stringValue;

        public bool GetBool()
        {
            return boolValue;
        }

        public int GetInt()
        {
            return intValue;
        }

        public float GetFloat()
        {
            return floatValue;
        }

        public UnityEngine.Object GetObject()
        {
            return objectValue;
        }

        public string GetString()
        {
            return stringValue;
        }

        public void SetValue(int value)
        {
            intValue = value;
        }

        public void SetValue(bool value)
        {
            boolValue = value;
        }
        public void SetValue(float value)
        {
            floatValue = value;
        }
        public void SetValue(string value)
        {
            stringValue = value;
        }
        public void SetValue(UnityEngine.Object value)
        {
            objectValue = value;
        }
    }
}