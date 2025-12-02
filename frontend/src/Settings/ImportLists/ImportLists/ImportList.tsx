import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import EditImportListModalConnector from './EditImportListModalConnector';
import styles from './ImportList.css';

// Define props interface
interface ImportListProps {
  id: number;
  name: string;
  enabled: boolean;
  enableAuto: boolean;
  minRefreshInterval: string;
  onConfirmDeleteImportList: (id: number) => void;
  lastInfoSync?: string;
}

function ImportList({
  id,
  name,
  enabled,
  enableAuto,
  minRefreshInterval,
  lastInfoSync,
  onConfirmDeleteImportList,
}: ImportListProps) {
  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  const [isEditImportListModalOpen, setIsEditImportListModalOpen] =
    useState(false);
  const [isDeleteImportListModalOpen, setIsDeleteImportListModalOpen] =
    useState(false);

  const handleEditImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(true);
  }, []);

  const handleEditImportListModalClose = useCallback(() => {
    setIsEditImportListModalOpen(false);
  }, []);

  const handleDeleteImportListPress = useCallback(() => {
    setIsEditImportListModalOpen(false);
    setIsDeleteImportListModalOpen(true);
  }, []);

  const handleDeleteImportListModalClose = useCallback(() => {
    setIsDeleteImportListModalOpen(false);
  }, []);

  const handleConfirmDeleteImportListHandler = useCallback(() => {
    onConfirmDeleteImportList(id);
  }, [id, onConfirmDeleteImportList]);

  return (
    <Card
      className={styles.list}
      overlayContent={true}
      onPress={handleEditImportListPress}
    >
      <div className={styles.name}>{name}</div>

      <div className={styles.enabled}>
        {enabled ? (
          <Label kind={kinds.SUCCESS}>{translate('Enabled')}</Label>
        ) : (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        )}

        {enableAuto && (
          <Label kind={kinds.SUCCESS}>{translate('AutomaticAdd')}</Label>
        )}
      </div>

      <div className={styles.enabled}>
        <Label kind={kinds.INFO} title="List Refresh Interval">
          {`${translate('Refresh')}: ${formatShortTimeSpan(
            minRefreshInterval
          )}`}
        </Label>
      </div>

      {lastInfoSync && (
        <div className={styles.enabled}>
          <Label kind={kinds.DEFAULT} title="List Refresh Time">
            {`${translate('Refreshed')}: ${getRelativeDate({
              date: lastInfoSync,
              shortDateFormat,
              showRelativeDates,
              timeFormat,
              includeSeconds: false,
              timeForToday: true,
            })}`}
          </Label>
        </div>
      )}

      <EditImportListModalConnector
        id={id}
        isOpen={isEditImportListModalOpen}
        onModalClose={handleEditImportListModalClose}
        onDeleteImportListPress={handleDeleteImportListPress}
      />

      <ConfirmModal
        isOpen={isDeleteImportListModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteImportList')}
        message={translate('DeleteImportListMessageText', { name })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteImportListHandler}
        onCancel={handleDeleteImportListModalClose}
      />
    </Card>
  );
}

export default ImportList;
