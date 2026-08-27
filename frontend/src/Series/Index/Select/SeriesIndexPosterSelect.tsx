import React, { SyntheticEvent, useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './SeriesIndexPosterSelect.css';

interface SeriesIndexPosterSelectProps {
  seriesId: number;
  titleSlug: string;
}

function SeriesIndexPosterSelect({
  seriesId,
  titleSlug,
}: SeriesIndexPosterSelectProps) {
  const [selectState, selectDispatch] = useSelect();
  const isSelected = selectState.selectedState[seriesId];

  const onSelectPress = useCallback(
    (event: SyntheticEvent<HTMLElement, PointerEvent>) => {
      if (event.nativeEvent.ctrlKey || event.nativeEvent.metaKey) {
        window.open(`${window.Whisparr.urlBase}/site/${titleSlug}`, '_blank');
        return;
      }

      const shiftKey = event.nativeEvent.shiftKey;

      selectDispatch({
        type: 'toggleSelected',
        id: seriesId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [seriesId, titleSlug, isSelected, selectDispatch]
  );

  return (
    <Link className={styles.checkButton} onPress={onSelectPress}>
      <span className={styles.checkContainer}>
        <Icon
          className={isSelected ? styles.selected : styles.unselected}
          name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
          size={20}
        />
      </span>
    </Link>
  );
}

export default SeriesIndexPosterSelect;
