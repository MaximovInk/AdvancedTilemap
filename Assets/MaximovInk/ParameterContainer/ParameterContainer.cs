using System.Collections.Generic;
using System.Linq;

namespace MaximovInk
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

        public virtual Parameter GetOrAddParameter(string ID, bool isHidden = true,
            ParameterType type = ParameterType.Int)
        {
            var param = GetParam(ID);
            param ??= AddNewParam(new Parameter() { name = ID, type = type, isHidden = isHidden });

            return param;
        }


        public Parameter GetParam(string name) => parameters.FirstOrDefault(n => n.name == name);
    }
}
