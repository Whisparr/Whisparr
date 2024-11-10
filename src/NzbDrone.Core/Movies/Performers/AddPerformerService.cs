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

namespace NzbDrone.Core.Movies.Performers
{
    public interface IAddPerformerService
    {
        Performer AddPerformer(Performer newPerformer, bool ignoreErrors = false);
        List<Performer> AddPerformers(List<Performer> newPerformers, bool ignoreErrors = false);
    }

    public class AddPerformerService : IAddPerformerService
    {
        private readonly IPerformerService _performerService;
        private readonly IProvideMovieInfo _performerInfo;
        private readonly Logger _logger;

        public AddPerformerService(IPerformerService performerService,
                                   IProvideMovieInfo performerInfo,
                                   Logger logger)
        {
            _performerService = performerService;
            _performerInfo = performerInfo;
            _logger = logger;
        }

        public Performer AddPerformer(Performer newPerformer, bool ignoreErrors = false)
        {
            Ensure.That(newPerformer, () => newPerformer).IsNotNull();

            newPerformer = AddSkyhookData(newPerformer);
            newPerformer = SetPropertiesAndValidate(newPerformer);

            _logger.Info("Adding Performer {0}", newPerformer.Name);

            try
            {
                _performerService.AddPerformer(newPerformer);
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    throw;
                }

                _logger.Debug("StashId {0} was not added due to Exception. {1}", newPerformer.ForeignId, ex.Message);
            }

            return newPerformer;
        }

        public List<Performer> AddPerformers(List<Performer> newPerformers, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var performersToAdd = new List<Performer>();
            var existingPerformerForeignIds = _performerService.AllPerformerForeignIds();

            foreach (var m in newPerformers)
            {
                _logger.Info("Adding Performer {0}", m.Name);

                try
                {
                    var performer = AddSkyhookData(m);
                    performer = SetPropertiesAndValidate(performer);

                    performer.Added = added;

                    if (existingPerformerForeignIds.Any(f => f == performer.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Performer already exists in database", m.ForeignId);
                        continue;
                    }

                    if (performersToAdd.Any(f => f.ForeignId == performer.ForeignId))
                    {
                        _logger.Debug("Foreign ID {0} was not added due to validation failure: Performer already exists on list", m.ForeignId);
                        continue;
                    }

                    performersToAdd.Add(performer);
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

                    _logger.Error("StashId {0} was not added due to Exception. {1}", m.ForeignId, ex.Message);
                }
            }

            return _performerService.AddPerformers(performersToAdd);
        }

        private Performer AddSkyhookData(Performer newPerformer)
        {
            var performer = new Performer();

            try
            {
                performer = _performerInfo.GetPerformerInfo(newPerformer.ForeignId);
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("StashID {0} was not found, it may have been removed from StashDb.", newPerformer.ForeignId);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("StashId", $"A performer with this ID was not found.", newPerformer.ForeignId)
                                              });
            }

            performer.ApplyChanges(newPerformer);

            return performer;
        }

        private Performer SetPropertiesAndValidate(Performer newPerformer)
        {
            newPerformer.CleanName = newPerformer.Name.CleanMovieTitle();
            newPerformer.SortName = MovieTitleNormalizer.Normalize(newPerformer.Name, newPerformer.ForeignId);
            newPerformer.Added = DateTime.UtcNow;

            return newPerformer;
        }
    }
}
