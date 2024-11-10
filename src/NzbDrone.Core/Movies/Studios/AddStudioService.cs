using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies.Studios
{
    public interface IAddStudioService
    {
        Studio AddStudio(Studio newStudio, bool ignoreErrors = false);
        List<Studio> AddStudios(List<Studio> newStudios, bool ignoreErrors = false);
    }

    public class AddStudioService : IAddStudioService
    {
        private readonly IStudioService _studioService;
        private readonly IProvideMovieInfo _studioInfo;
        private readonly Logger _logger;

        public AddStudioService(IStudioService studioService,
                                   IProvideMovieInfo studioInfo,
                                   Logger logger)
        {
            _studioService = studioService;
            _studioInfo = studioInfo;
            _logger = logger;
        }

        public Studio AddStudio(Studio newStudio, bool ignoreErrors = false)
        {
            Ensure.That(newStudio, () => newStudio).IsNotNull();

            var existingStudio = _studioService.FindByForeignId(newStudio.ForeignId);

            if (existingStudio != null)
            {
                return existingStudio;
            }

            newStudio = AddSkyhookData(newStudio);
            newStudio = SetPropertiesAndValidate(newStudio);

            _logger.Info("Adding Studio {0}", newStudio.Title);

            try
            {
                _studioService.AddStudio(newStudio);
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    throw;
                }

                _logger.Error("Error Adding Studio {0}", newStudio.Title, ex);
            }

            return newStudio;
        }

        public List<Studio> AddStudios(List<Studio> newStudios, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var studiosToAdd = new List<Studio>();
            var existingStudioForeignIds = _studioService.AllStudioForeignIds();

            foreach (var m in newStudios)
            {
                _logger.Info("Adding Studio {0}", m.Title);

                try
                {
                    var studio = AddSkyhookData(m);
                    studio = SetPropertiesAndValidate(studio);

                    studio.Added = added;

                    if (existingStudioForeignIds.Any(f => f == studio.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Studio already exists in database", m.ForeignId);
                        continue;
                    }

                    if (studiosToAdd.Any(f => f.ForeignId == studio.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Studio already exists on list", m.ForeignId);
                        continue;
                    }

                    studiosToAdd.Add(studio);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Error("StashId {0} was not added due to validation failures. {1}", m.ForeignId, ex.Message);
                }
                catch (Exception ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Error("StashId {0} was not added due to an exception. {1}", m.ForeignId, ex.Message);
                }
            }

            return _studioService.AddStudios(studiosToAdd);
        }

        private Studio AddSkyhookData(Studio newStudio)
        {
            var studio = new Studio();

            try
            {
                studio = _studioInfo.GetStudioInfo(newStudio.ForeignId);
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("StashID {0} was not found, it may have been removed from StashDb.", newStudio.ForeignId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("StashId", $"A studio with this ID was not found.", newStudio.ForeignId)
                                              });
            }

            studio.ApplyChanges(newStudio);

            return studio;
        }

        private Studio SetPropertiesAndValidate(Studio newStudio)
        {
            newStudio.CleanTitle = newStudio.Title.CleanStudioTitle();
            newStudio.SortTitle = MovieTitleNormalizer.Normalize(newStudio.Title, newStudio.ForeignId);
            newStudio.Added = DateTime.UtcNow;

            return newStudio;
        }
    }
}
