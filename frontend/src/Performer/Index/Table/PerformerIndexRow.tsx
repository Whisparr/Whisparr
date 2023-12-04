import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import TagListConnector from 'Components/TagListConnector';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds } from 'Helpers/Props';
import PerformerDetailsLinks from 'Performer/Details/PerformerDetailsLinks';
import EditPerformerModalConnector from 'Performer/Edit/EditPerformerModalConnector';
import createPerformerIndexItemSelector from 'Performer/Index/createPerformerIndexItemSelector';
import PerformerNameLink from 'Performer/PerformerNameLink';
import { SelectStateInputProps } from 'typings/props';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './PerformerIndexRow.css';

interface PerformerIndexRowProps {
  performerId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function PerformerIndexRow(props: PerformerIndexRowProps) {
  const { performerId, columns, isSelectMode } = props;

  const { performer, qualityProfile } = useSelector(
    createPerformerIndexItemSelector(performerId)
  );

  const {
    fullName,
    monitored,
    gender,
    hairColor,
    ethnicity,
    rootFolderPath,
    tags = [],
    foreignId,
  } = performer;

  const [isEditPerformerModalOpen, setIsEditPerformerModalOpen] =
    useState(false);
  const [selectState, selectDispatch] = useSelect();

  const onEditPerformerPress = useCallback(() => {
    setIsEditPerformerModalOpen(true);
  }, [setIsEditPerformerModalOpen]);

  const onEditPerformerModalClose = useCallback(() => {
    setIsEditPerformerModalOpen(false);
  }, [setIsEditPerformerModalOpen]);

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={performerId}
          isSelected={selectState.selectedState[performerId]}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <Icon name={monitored ? icons.MONITORED : icons.UNMONITORED} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'fullName') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <PerformerNameLink foreignId={foreignId} title={fullName} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'gender') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {firstCharToUpper(gender)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'hairColor') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {firstCharToUpper(hairColor)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'ethnicity') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {firstCharToUpper(ethnicity)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfile?.name ?? ''}
            </VirtualTableRowCell>
          );
        }

        if (name === 'rootFolderPath') {
          return (
            <VirtualTableRowCell
              key={name}
              className={styles[name]}
              title={rootFolderPath}
            >
              {rootFolderPath}
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TagListConnector tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span className={styles.externalLinks}>
                <Tooltip
                  anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                  tooltip={<PerformerDetailsLinks foreignId={foreignId} />}
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <IconButton
                name={icons.EDIT}
                title={translate('EditPerformer')}
                onPress={onEditPerformerPress}
              />
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

      <EditPerformerModalConnector
        isOpen={isEditPerformerModalOpen}
        performerId={performerId}
        onModalClose={onEditPerformerModalClose}
      />
    </>
  );
}

export default PerformerIndexRow;
