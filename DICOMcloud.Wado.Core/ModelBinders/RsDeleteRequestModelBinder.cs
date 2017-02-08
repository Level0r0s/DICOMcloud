﻿
using DICOMcloud.Wado.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace DICOMcloud.Wado.Core
{
    public class RsDeleteRequestModelBinder : RsRequestModelBinder<IWebDeleteRequest> 
    {
        protected override RsRequestModelConverter<IWebDeleteRequest> GetConverter ( )
        {
            return new DeleteRsRequestModelConverter ( ) ;
        }
    }
}
