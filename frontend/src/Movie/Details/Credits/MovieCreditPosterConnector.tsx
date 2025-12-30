import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Performer from 'Performer/Performer';
import { togglePerformerMonitored } from 'Store/Actions/performerActions';

interface Props {
  performerForeignId: string;
  performer?: Performer;
  component: React.ElementType;
  posterWidth?: number;
  posterHeight?: number;
  job?: string;
  character?: string;
}

const selectPerformers = (state: AppState) =>
  state.performers.items as Performer[];
const selectSafeForWork = (state: AppState) => state.settings.safeForWorkMode;

function MovieCreditPosterConnector(props: Props) {
  const { performerForeignId, performer: performerProp } = props;
  const performers = useSelector(selectPerformers);
  const safeForWorkMode = useSelector(selectSafeForWork);
  const dispatch = useDispatch();

  const performer =
    performers?.find((p) => p.foreignId === performerForeignId) ||
    performerProp;

  const onTogglePerformerMonitored = useCallback(
    (monitored: boolean) => {
      if (!performer) return;
      dispatch(
        togglePerformerMonitored({
          performerId: performer.id,
          monitored,
        })
      );
    },
    [dispatch, performer]
  );

  const ItemComponent = props.component;

  return (
    <ItemComponent
      {...props}
      performer={performer}
      safeForWorkMode={safeForWorkMode}
      onTogglePerformerMonitored={onTogglePerformerMonitored}
    />
  );
}

export default MovieCreditPosterConnector;
