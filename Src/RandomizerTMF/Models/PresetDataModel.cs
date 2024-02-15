using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerTMF.Models
{
    internal class PresetDataModel
    {
        public string FolderName { get; set; }
        public RandomizerRules Data { get; }

        public PresetDataModel(string FolderName, RandomizerRules Data)
        {
            this.FolderName = FolderName;
            this.Data = Data;
        }
    }
}