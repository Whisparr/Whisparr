import { orderBy } from 'lodash';
import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RENAME_MOVIE } from 'Commands/commandNames';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllScenesSelector from 'Store/Selectors/createAllScenesSelector';
import translate from 'Utilities/String/translate';
import styles from './OrganizeScenesModalContent.css';

interface OrganizeScenesModalContentProps {
  sceneIds: number[];
  onModalClose: () => void;
}

function OrganizeScenesModalContent(props: OrganizeScenesModalContentProps) {
  const { sceneIds, onModalClose } = props;

  const allScenes: Movie[] = useSelector(createAllScenesSelector());
  const dispatch = useDispatch();

  const sceneTitles = useMemo(() => {
    const scene = sceneIds.reduce((acc: Movie[], id) => {
      const s = allScenes.find((s) => s.id === id);

      if (s) {
        acc.push(s);
      }

      return acc;
    }, []);

    const sorted = orderBy(scene, ['sortTitle']);

    return sorted.map((s) => s.title);
  }, [sceneIds, allScenes]);

  const onOrganizePress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RENAME_MOVIE,
        movieIds: sceneIds,
      })
    );

    onModalClose();
  }, [sceneIds, onModalClose, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('OrganizeSelectedScenes')}</ModalHeader>

      <ModalBody>
        <Alert>
          {translate('PreviewRenameHelpText')}
          <Icon className={styles.renameIcon} name={icons.ORGANIZE} />
        </Alert>

        <div className={styles.message}>
          {translate('OrganizeConfirm', { count: sceneTitles.length })}
        </div>

        <ul>
          {sceneTitles.map((title) => {
            return <li key={title}>{title}</li>;
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onOrganizePress}>
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default OrganizeScenesModalContent;
