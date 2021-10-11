﻿using Landis.Library.Metadata;

namespace Landis.Extension.BaseHurricane
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Year")]
        public int Year { set; get; }

        [DataFieldAttribute(Desc = "Damaged Sites")]
        public int NumberEvents { set; get; }

    }
}
