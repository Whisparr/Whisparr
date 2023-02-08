using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Stash
{
    public interface IStashService
    {
        void GetStatus(StashSettings settings);
        void Update(StashSettings settings, Series series);
        ValidationFailure Test(StashSettings settings);
    }

    public class StashService : IStashService
    {
        private readonly StashProxy _proxy;
        private readonly Logger _logger;

        public StashService(StashProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public void Update(StashSettings settings, Series series)
        {
            var seriesLocation = new OsPath(series.Path);
            var mappedPath = seriesLocation;

            if (settings.MapTo.IsNotNullOrWhiteSpace())
            {
                mappedPath = new OsPath(settings.MapTo) + (seriesLocation - new OsPath(settings.MapFrom));

                _logger.Trace("Mapping Path from {0} to {1} for partial scan", seriesLocation, mappedPath);
            }

            _proxy.Update(settings, mappedPath.FullPath);
        }

        public void GetStatus(StashSettings settings)
        {
            _proxy.GetStatus(settings);
        }

        public ValidationFailure Test(StashSettings settings)
        {
            try
            {
                _logger.Debug("Testing connection to Stash: {0}", settings.Address);

                GetStatus(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ValidationFailure("ApiKey", "API Key is incorrect");
                }

                return new ValidationFailure("Host", "Unable to send test message: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message: " + ex.Message);
            }

            return null;
        }
    }
}
