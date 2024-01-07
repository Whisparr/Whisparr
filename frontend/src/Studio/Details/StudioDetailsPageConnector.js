import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import NotFound from 'Components/NotFound';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import StudioDetailsConnector from './StudioDetailsConnector';
import styles from './StudioDetails.css';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.studios,
    (match, studios) => {
      const foreignId = match.params.foreignId;
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = studios;

      const movieIndex = _.findIndex(items, { foreignId });

      if (movieIndex > -1) {
        return {
          isFetching,
          isPopulated,
          foreignId
        };
      }

      return {
        isFetching,
        isPopulated,
        error
      };
    }
  );
}

const mapDispatchToProps = {
  push,
  fetchRootFolders
};

class StudioDetailsPageConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRootFolders();
  }

  componentDidUpdate(prevProps) {
    if (!this.props.foreignId) {
      this.props.push(`${window.Whisparr.urlBase}/`);
      return;
    }
  }

  //
  // Render

  render() {
    const {
      foreignId,
      isFetching,
      isPopulated,
      error
    } = this.props;

    if (isFetching && !isPopulated) {
      return (
        <PageContent title={translate('Loading')}>
          <PageContentBody>
            <LoadingIndicator />
          </PageContentBody>
        </PageContent>
      );
    }

    if (!isFetching && !!error) {
      return (
        <div className={styles.errorMessage}>
          {getErrorMessage(error, translate('FailedToLoadStudioFromAPI'))}
        </div>
      );
    }

    if (!foreignId) {
      return (
        <NotFound
          message={translate('SorryThatStudioCannotBeFound')}
        />
      );
    }

    return (
      <StudioDetailsConnector
        foreignId={foreignId}
      />
    );
  }
}

StudioDetailsPageConnector.propTypes = {
  foreignId: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  match: PropTypes.shape({ params: PropTypes.shape({ foreignId: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(StudioDetailsPageConnector);
