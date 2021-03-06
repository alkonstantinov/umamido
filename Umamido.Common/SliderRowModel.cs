﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umamido.Common
{
    public class SliderRowModel
    {
        public int SliderId { get; set; }

        public int ImageId { get; set; }

        public String FirstTitle { get; set; }
        public String FirstTitleNoHtml { get { return Tools.Tools.StripHtmlTags(FirstTitle); } }
        
        public String FirstDescription { get; set; }
        public String FirstDescriptionNoHtml { get { return Tools.Tools.StripHtmlTags(FirstDescription); } }

        public String ButtonUrl { get; set; }

        public Boolean IsActive { get; set; }

        public TranslatableItemModel[] Titles { get; set; }

        public TranslatableItemModel[] Descriptions { get; set; }

    }
}
