import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteStudio, setDeleteOption } from 'Store/Actions/studioActions';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import DeleteStudioModalContent from './DeleteStudioModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.studios.deleteOptions,
    createStudioSelector(),
    (deleteOptions, studio) => {
      return {
        ...studio,
        deleteOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteOptionChange(option) {
      dispatch(
        setDeleteOption({
          [option.name]: option.value
        })
      );
    },

    onDeletePress(deleteFiles, addImportExclusion) {
      dispatch(
        deleteStudio({
          id: props.studioId,
          deleteFiles,
          addImportExclusion
        })
      );

      props.onModalClose(true);
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteStudioModalContent);
