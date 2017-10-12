using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZcrlMedicamentModels;

namespace ZcrlPortal.ViewModels
{
    public class MedGroup
    {
        public MedicamentGroup Name { get; set; }
        public List<MedicamentRemain> MedicamentsRemains { get; set; }
    }
}