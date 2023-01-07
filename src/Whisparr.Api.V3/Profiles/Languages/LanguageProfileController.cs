using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Profiles.Languages
{
    [V3ApiController]
    [Obsolete("Deprecated")]
    public class LanguageProfileController : RestController<LanguageProfileResource>
    {
        [RestPostById]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult<LanguageProfileResource> Create(LanguageProfileResource resource)
        {
            return Accepted(resource);
        }

        [RestDeleteById]
        public void DeleteProfile(int id)
        {
        }

        [RestPutById]
        [Produces("application/json")]
        [Consumes("application/json")]
        public ActionResult<LanguageProfileResource> Update(LanguageProfileResource resource)
        {
            return Accepted(resource);
        }

        [RestGetById]
        [Produces("application/json")]
        protected override LanguageProfileResource GetResourceById(int id)
        {
            return new LanguageProfileResource
            {
                Id = 1,
                Name = "Deprecated",
                UpgradeAllowed = true,
                Cutoff = Language.English,
                Languages = new List<LanguageProfileItemResource>
                {
                    new LanguageProfileItemResource
                    {
                        Language = Language.English,
                        Allowed = true
                    }
                }
            };
        }

        [HttpGet]
        [Produces("application/json")]
        public ActionResult<List<LanguageProfileResource>> GetAll()
        {
            return new List<LanguageProfileResource>
            {
                new LanguageProfileResource
                {
                    Id = 1,
                    Name = "Deprecated",
                    UpgradeAllowed = true,
                    Cutoff = Language.English,
                    Languages = new List<LanguageProfileItemResource>
                    {
                        new LanguageProfileItemResource
                        {
                            Language = Language.English,
                            Allowed = true
                        }
                    }
                }
            };
        }
    }
}
