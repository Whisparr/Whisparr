import PropTypes from 'prop-types';
import React from 'react';
import { Redirect, Route } from 'react-router-dom';
import BlocklistConnector from 'Activity/Blocklist/BlocklistConnector';
import HistoryConnector from 'Activity/History/HistoryConnector';
import QueueConnector from 'Activity/Queue/QueueConnector';
import AddNewMovieConnector from 'AddMovie/AddNewMovie/AddNewMovieConnector';
import AddNewSceneConnector from 'AddMovie/AddNewMovie/AddNewSceneConnector';
import ImportMovies from 'AddMovie/ImportMovie/ImportMovies';
import CalendarPageConnector from 'Calendar/CalendarPageConnector';
import NotFound from 'Components/NotFound';
import Switch from 'Components/Router/Switch';
import MovieDetailsPageConnector from 'Movie/Details/MovieDetailsPageConnector';
import MovieIndex from 'Movie/Index/MovieIndex';
import PerformerIndex from 'Performer/Index/PerformerIndex';
import SceneIndex from 'Scene/Index/SceneIndex';
import CustomFormatSettingsPage from 'Settings/CustomFormats/CustomFormatSettingsPage';
import DownloadClientSettingsConnector from 'Settings/DownloadClients/DownloadClientSettingsConnector';
import GeneralSettingsConnector from 'Settings/General/GeneralSettingsConnector';
import ImportListSettingsConnector from 'Settings/ImportLists/ImportListSettingsConnector';
import IndexerSettingsConnector from 'Settings/Indexers/IndexerSettingsConnector';
import MediaManagementConnector from 'Settings/MediaManagement/MediaManagementConnector';
import MetadataSettings from 'Settings/Metadata/MetadataSettings';
import NotificationSettings from 'Settings/Notifications/NotificationSettings';
import Profiles from 'Settings/Profiles/Profiles';
import QualityConnector from 'Settings/Quality/QualityConnector';
import Settings from 'Settings/Settings';
import TagSettings from 'Settings/Tags/TagSettings';
import UISettingsConnector from 'Settings/UI/UISettingsConnector';
import StudioIndex from 'Studio/Index/StudioIndex';
import BackupsConnector from 'System/Backup/BackupsConnector';
import LogsTableConnector from 'System/Events/LogsTableConnector';
import Logs from 'System/Logs/Logs';
import Status from 'System/Status/Status';
import Tasks from 'System/Tasks/Tasks';
import UpdatesConnector from 'System/Updates/UpdatesConnector';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';

function AppRoutes(props) {
  const {
    app
  } = props;

  return (
    <Switch>
      {/*
        Movies
      */}

      <Route
        exact={true}
        path="/"
        component={MovieIndex}
      />

      {
        window.Whisparr.urlBase &&
          <Route
            exact={true}
            path="/"
            addUrlBase={false}
            render={() => {
              return (
                <Redirect
                  to={getPathWithUrlBase('/')}
                  component={app}
                />
              );
            }}
          />
      }

      <Route
        path="/movies"
        component={MovieIndex}
      />

      <Route
        path="/scenes"
        component={SceneIndex}
      />

      <Route
        path="/performers"
        component={PerformerIndex}
      />

      <Route
        path="/studios"
        component={StudioIndex}
      />

      <Route
        path="/add/new/movie"
        component={AddNewMovieConnector}
      />

      <Route
        path="/add/new/scene"
        component={AddNewSceneConnector}
      />

      <Route
        path="/add/import"
        component={ImportMovies}
      />

      <Route
        path="/movie/:titleSlug"
        component={MovieDetailsPageConnector}
      />

      {/*
        Calendar
      */}

      <Route
        path="/calendar"
        component={CalendarPageConnector}
      />

      {/*
        Activity
      */}

      <Route
        path="/activity/history"
        component={HistoryConnector}
      />

      <Route
        path="/activity/queue"
        component={QueueConnector}
      />

      <Route
        path="/activity/blocklist"
        component={BlocklistConnector}
      />

      {/*
        Settings
      */}

      <Route
        exact={true}
        path="/settings"
        component={Settings}
      />

      <Route
        path="/settings/mediamanagement"
        component={MediaManagementConnector}
      />

      <Route
        path="/settings/profiles"
        component={Profiles}
      />

      <Route
        path="/settings/quality"
        component={QualityConnector}
      />

      <Route
        path="/settings/customformats"
        component={CustomFormatSettingsPage}
      />

      <Route
        path="/settings/indexers"
        component={IndexerSettingsConnector}
      />

      <Route
        path="/settings/downloadclients"
        component={DownloadClientSettingsConnector}
      />

      <Route
        path="/settings/importlists"
        component={ImportListSettingsConnector}
      />

      <Route
        path="/settings/connect"
        component={NotificationSettings}
      />

      <Route
        path="/settings/metadata"
        component={MetadataSettings}
      />

      <Route
        path="/settings/tags"
        component={TagSettings}
      />

      <Route
        path="/settings/general"
        component={GeneralSettingsConnector}
      />

      <Route
        path="/settings/ui"
        component={UISettingsConnector}
      />

      {/*
        System
      */}

      <Route
        path="/system/status"
        component={Status}
      />

      <Route
        path="/system/tasks"
        component={Tasks}
      />

      <Route
        path="/system/backup"
        component={BackupsConnector}
      />

      <Route
        path="/system/updates"
        component={UpdatesConnector}
      />

      <Route
        path="/system/events"
        component={LogsTableConnector}
      />

      <Route
        path="/system/logs/files"
        component={Logs}
      />

      {/*
        Not Found
      */}

      <Route
        path="*"
        component={NotFound}
      />
    </Switch>
  );
}

AppRoutes.propTypes = {
  app: PropTypes.func.isRequired
};

export default AppRoutes;
