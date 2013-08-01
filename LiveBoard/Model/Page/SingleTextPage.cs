﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveBoard.Model.Page
{
    /// <summary>
    /// 페이지 인터페이스
    /// </summary>
    public class SingleTextPage: IPage
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateOption { get; set; }
        public TimeSpan Duration { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }

        /// <summary>
        /// Specific data.
        /// </summary>
        public string Data { get; set; }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Title, TemplateCode);
        }
    }
}