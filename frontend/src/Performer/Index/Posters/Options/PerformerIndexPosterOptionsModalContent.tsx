import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import { setPerformerPosterOption } from 'Store/Actions/performerActions';
import translate from 'Utilities/String/translate';
import selectPosterOptions from '../selectPosterOptions';

const posterSizeOptions = [
  {
    key: 'small',
    get value() {
      return translate('Small');
    },
  },
  {
    key: 'medium',
    get value() {
      return translate('Medium');
    },
  },
  {
    key: 'large',
    get value() {
      return translate('Large');
    },
  },
];

interface PerformerIndexPosterOptionsModalContentProps {
  onModalClose(...args: unknown[]): unknown;
}

function PerformerIndexPosterOptionsModalContent(
  props: PerformerIndexPosterOptionsModalContentProps
) {
  const { onModalClose } = props;

  const posterOptions = useSelector(selectPosterOptions);

  const { detailedProgressBar, size, showName } = posterOptions;

  const dispatch = useDispatch();

  const onPosterOptionChange = useCallback(
    ({ name, value }: { name: string; value: unknown }) => {
      dispatch(setPerformerPosterOption({ [name]: value }));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('PosterOptions')}</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>{translate('PosterSize')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="size"
              value={size}
              values={posterSizeOptions}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('DetailedProgressBar')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="detailedProgressBar"
              value={detailedProgressBar}
              helpText={translate('DetailedProgressBarHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowName')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showName"
              value={showName}
              helpText={translate('ShowNameHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default PerformerIndexPosterOptionsModalContent;
