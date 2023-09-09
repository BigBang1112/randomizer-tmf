using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerTMF.Models
{
    public class PresetDataModel
    {
        public string FolderName { get; set; }
        public List<string> FileContent { get; set; }

        public PresetDataModel(string FolderName, List<string> FileContent) 
        {
            this.FolderName = FolderName;
            this.FileContent = FileContent;
        }
    }
}
