import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { createSelector } from 'reselect';
import { deleteStudio, setDeleteOption } from 'Store/Actions/studioActions';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import DeleteStudioModalContent from './DeleteStudioModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.studios.deleteOptions,
    createStudioSelector(),
    (deleteOptions, studioIds) => {
      return {
        deleteOptions,
        studioIds
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
      props.studioIds.forEach((id) => {
        dispatch(
          deleteStudio({
            id,
            deleteFiles,
            addImportExclusion
          })
        );
      });

      props.onModalClose(true);
      props.history.push('/studios');
    }
  };
}

export default withRouter(connect(createMapStateToProps, createMapDispatchToProps)(DeleteStudioModalContent));
