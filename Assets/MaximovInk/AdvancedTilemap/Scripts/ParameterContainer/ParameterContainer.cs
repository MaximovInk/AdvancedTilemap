using System.Collections.Generic;
using System.Linq;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class ParameterContainer
    {
        public List<Parameter> parameters = new();

        public Parameter AddNewParam(Parameter param)
        {
            parameters.Add(param);

            return param;
        }

        public void RemoveParam(string name)
        {
            parameters.RemoveAll(n => n.name == name);
        }

        public Parameter GetParam(string name) => parameters.FirstOrDefault(n => n.name == name);
    }
}
