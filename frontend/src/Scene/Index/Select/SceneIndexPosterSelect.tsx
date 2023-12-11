import React, { SyntheticEvent, useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './SceneIndexPosterSelect.css';

interface SceneIndexPosterSelectProps {
  sceneId: number;
}

function SceneIndexPosterSelect(props: SceneIndexPosterSelectProps) {
  const { sceneId } = props;
  const [selectState, selectDispatch] = useSelect();
  const isSelected = selectState.selectedState[sceneId];

  const onSelectPress = useCallback(
    (event: SyntheticEvent) => {
      const nativeEvent = event.nativeEvent as PointerEvent;
      const shiftKey = nativeEvent.shiftKey;

      selectDispatch({
        type: 'toggleSelected',
        id: sceneId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [sceneId, isSelected, selectDispatch]
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

export default SceneIndexPosterSelect;
