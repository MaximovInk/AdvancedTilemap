using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class ParameterContainer
    {
        public List<Parameter> parameters = new List<Parameter>();

        public void AddNewParam(Parameter param)
        {
            parameters.Add(param);
        }

        public void RemoveParam(string name)
        {
            parameters.RemoveAll(n => n.name == name);
        }

        public Parameter GetParam(string name) => parameters.FirstOrDefault(n => n.name == name);
    }
}
