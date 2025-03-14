import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { createSelector } from 'reselect';
import { deletePerformer, setDeleteOption } from 'Store/Actions/performerActions';
import createPerformerSelector from 'Store/Selectors/createPerformerSelector';
import DeletePerformerModalContent from './DeletePerformerModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.performers.deleteOptions,
    createPerformerSelector(),
    (deleteOptions, performer) => {
      return {
        ...performer,
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
        deletePerformer({
          id: props.performerId,
          deleteFiles,
          addImportExclusion
        })
      );

      props.onModalClose(true);
      props.history.push('/performers');
    }
  };
}

export default withRouter(connect(createMapStateToProps, createMapDispatchToProps)(DeletePerformerModalContent));
