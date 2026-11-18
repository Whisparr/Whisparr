import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import LogFiles from '../LogFiles';
import { useUpdateLogFiles } from '../useLogFiles';

function UpdateLogFiles() {
  const dispatch = useDispatch();
  const { data = [], isFetching, refetch } = useUpdateLogFiles();

  const isDeleteFilesExecuting = useSelector(
    createCommandExecutingSelector(commandNames.DELETE_UPDATE_LOG_FILES)
  );

  const handleRefreshPress = useCallback(() => {
    refetch();
  }, [refetch]);

  const handleDeleteFilesPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.DELETE_UPDATE_LOG_FILES,
        commandFinished: () => {
          refetch();
        },
      })
    );
  }, [dispatch, refetch]);

  return (
    <LogFiles
      isDeleteFilesExecuting={isDeleteFilesExecuting}
      isFetching={isFetching}
      items={data}
      type="update"
      onRefreshPress={handleRefreshPress}
      onDeleteFilesPress={handleDeleteFilesPress}
    />
  );
}

export default UpdateLogFiles;
