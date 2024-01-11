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
import StudioDetailsLinks from 'Studio/Details/StudioDetailsLinks';
import EditStudioModalConnector from 'Studio/Edit/EditStudioModalConnector';
import createStudioIndexItemSelector from 'Studio/Index/createStudioIndexItemSelector';
import StudioTitleLink from 'Studio/StudioTitleLink';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './StudioIndexRow.css';

interface StudioIndexRowProps {
  studioId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function StudioIndexRow(props: StudioIndexRowProps) {
  const { studioId, columns, isSelectMode } = props;

  const { studio, qualityProfile } = useSelector(
    createStudioIndexItemSelector(studioId)
  );

  const {
    title,
    network,
    monitored,
    rootFolderPath,
    tags = [],
    foreignId,
    website,
  } = studio;

  const [isEditStudioModalOpen, setIsEditStudioModalOpen] = useState(false);
  const [selectState, selectDispatch] = useSelect();

  const onEditStudioPress = useCallback(() => {
    setIsEditStudioModalOpen(true);
  }, [setIsEditStudioModalOpen]);

  const onEditStudioModalClose = useCallback(() => {
    setIsEditStudioModalOpen(false);
  }, [setIsEditStudioModalOpen]);

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
          id={studioId}
          isSelected={selectState.selectedState[studioId]}
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

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <StudioTitleLink foreignId={foreignId} title={title} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'network') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {network}
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
                  tooltip={
                    <StudioDetailsLinks
                      foreignId={foreignId}
                      website={website}
                    />
                  }
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <IconButton
                name={icons.EDIT}
                title={translate('EditStudio')}
                onPress={onEditStudioPress}
              />
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

      <EditStudioModalConnector
        isOpen={isEditStudioModalOpen}
        studioId={studioId}
        onModalClose={onEditStudioModalClose}
      />
    </>
  );
}

export default StudioIndexRow;
