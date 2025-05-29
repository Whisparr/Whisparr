import React from 'react';
import { Redirect, Route } from 'react-router-dom';
import Blocklist from 'Activity/Blocklist/Blocklist';
import HistoryConnector from 'Activity/History/HistoryConnector';
import Queue from 'Activity/Queue/Queue';
import AddNewMovieConnector from 'AddMovie/AddNewMovie/AddNewMovieConnector';
import AddNewPerformerConnector from 'AddMovie/AddNewMovie/AddNewPerformerConnector';
import AddNewSceneConnector from 'AddMovie/AddNewMovie/AddNewSceneConnector';
import AddNewStudioConnector from 'AddMovie/AddNewMovie/AddNewStudioConnector';
import ImportMovies from 'AddMovie/ImportMovie/ImportMovies';
import CalendarPage from 'Calendar/CalendarPage';
import NotFound from 'Components/NotFound';
import Switch from 'Components/Router/Switch';
import MovieDetailsPageConnector from 'Movie/Details/MovieDetailsPageConnector';
import MovieIndex from 'Movie/Index/MovieIndex';
import PerformerDetailsPageConnector from 'Performer/Details/PerformerDetailsPageConnector';
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
import StudioDetailsPageConnector from 'Studio/Details/StudioDetailsPageConnector';
import StudioIndex from 'Studio/Index/StudioIndex';
import BackupsConnector from 'System/Backup/BackupsConnector';
import LogsTableConnector from 'System/Events/LogsTableConnector';
import Logs from 'System/Logs/Logs';
import Status from 'System/Status/Status';
import Tasks from 'System/Tasks/Tasks';
import Updates from 'System/Updates/Updates';
import UnmappedFilesTableConnector from 'UnmappedFiles/UnmappedFilesTableConnector';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import CutoffUnmet from 'Wanted/CutoffUnmet/CutoffUnmet';
import Missing from 'Wanted/Missing/Missing';

function RedirectWithUrlBase() {
  return <Redirect to={getPathWithUrlBase('/')} />;
}

function AppRoutes() {
  return (
    <Switch>
      {/*
        Movies
      */}

      <Route exact={true} path="/" component={SceneIndex} />

      {window.Whisparr.urlBase && (
        <Route
          exact={true}
          path="/"
          // eslint-disable-next-line @typescript-eslint/ban-ts-comment
          // @ts-ignore
          addUrlBase={false}
          render={RedirectWithUrlBase}
        />
      )}

      <Route path="/movies" component={MovieIndex} />

      <Route path="/scenes" component={SceneIndex} />

      <Route path="/performers" component={PerformerIndex} />

      <Route path="/studios" component={StudioIndex} />

      <Route path="/add/new/movie" component={AddNewMovieConnector} />

      <Route path="/add/new/scene" component={AddNewSceneConnector} />

      <Route path="/add/new/studio" component={AddNewStudioConnector} />

      <Route path="/add/new/performer" component={AddNewPerformerConnector} />

      <Route path="/add/import" component={ImportMovies} />

      <Route path="/movie/:titleSlug" component={MovieDetailsPageConnector} />

      <Route
        path="/performer/:foreignId"
        component={PerformerDetailsPageConnector}
      />

      <Route path="/studio/:foreignId" component={StudioDetailsPageConnector} />

      <Route path="/unmapped" component={UnmappedFilesTableConnector} />

      {/*
        Calendar
      */}

      <Route path="/calendar" component={CalendarPage} />

      {/*
        Activity
      */}

      <Route path="/activity/history" component={HistoryConnector} />

      <Route path="/activity/queue" component={Queue} />

      <Route path="/activity/blocklist" component={Blocklist} />

      {/*
        Wanted
      */}

      <Route path="/wanted/missing" component={Missing} />

      <Route path="/wanted/cutoffunmet" component={CutoffUnmet} />

      {/*
        Settings
      */}

      <Route exact={true} path="/settings" component={Settings} />

      <Route
        path="/settings/mediamanagement"
        component={MediaManagementConnector}
      />

      <Route path="/settings/profiles" component={Profiles} />

      <Route path="/settings/quality" component={QualityConnector} />

      <Route
        path="/settings/customformats"
        component={CustomFormatSettingsPage}
      />

      <Route path="/settings/indexers" component={IndexerSettingsConnector} />

      <Route
        path="/settings/downloadclients"
        component={DownloadClientSettingsConnector}
      />

      <Route
        path="/settings/importlists"
        component={ImportListSettingsConnector}
      />

      <Route path="/settings/connect" component={NotificationSettings} />

      <Route path="/settings/metadata" component={MetadataSettings} />

      <Route path="/settings/tags" component={TagSettings} />

      <Route path="/settings/general" component={GeneralSettingsConnector} />

      <Route path="/settings/ui" component={UISettingsConnector} />

      {/*
        System
      */}

      <Route path="/system/status" component={Status} />

      <Route path="/system/tasks" component={Tasks} />

      <Route path="/system/backup" component={BackupsConnector} />

      <Route path="/system/updates" component={Updates} />

      <Route path="/system/events" component={LogsTableConnector} />

      <Route path="/system/logs/files" component={Logs} />

      {/*
        Not Found
      */}

      <Route path="*" component={NotFound} />
    </Switch>
  );
}

export default AppRoutes;
