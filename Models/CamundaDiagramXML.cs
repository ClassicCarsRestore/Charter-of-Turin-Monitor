using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class CamundaDiagramXML
    {
        public string Id { get; set; }

        public string Bpmn20Xml { get; set; }

        
        public string DiagramId
        {
            get
            {
                int indexOfVariable = Bpmn20Xml.IndexOf("<process id=") + 13;
                int indexOfEndOfValue = Bpmn20Xml.IndexOf(" ", indexOfVariable) - 1;
                int length = indexOfEndOfValue - indexOfVariable;
                // process id="restoration_evaluation_general" name="
                if (indexOfVariable != -1 && indexOfEndOfValue != -1)
                    return Bpmn20Xml.Substring(indexOfVariable, length);

                return "";
            }
        }
        
    }
}
