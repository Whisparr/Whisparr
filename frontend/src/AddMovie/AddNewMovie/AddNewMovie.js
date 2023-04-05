import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import AddNewMovieSearchResultConnector from './AddNewMovieSearchResultConnector';
import styles from './AddNewMovie.css';

class AddNewMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      term: props.term || '',
      isFetching: false
    };
  }

  componentDidMount() {
    const term = this.state.term;

    if (term) {
      this.props.onMovieLookupChange(term);
    }
  }

  componentDidUpdate(prevProps) {
    const {
      term,
      isFetching
    } = this.props;

    if (term && term !== prevProps.term) {
      this.setState({
        term,
        isFetching: true
      });
      this.props.onMovieLookupChange(term);
    } else if (isFetching !== prevProps.isFetching) {
      this.setState({
        isFetching
      });
    }
  }

  //
  // Listeners

  onSearchInputChange = ({ value }) => {
    const hasValue = !!value.trim();

    this.setState({ term: value, isFetching: hasValue }, () => {
      if (hasValue) {
        this.props.onMovieLookupChange(value);
      } else {
        this.props.onClearMovieLookup();
      }
    });
  };

  onClearMovieLookupPress = () => {
    this.setState({ term: '' });
    this.props.onClearMovieLookup();
  };

  //
  // Render

  render() {
    const {
      error,
      items,
      hasExistingMovie
    } = this.props;

    const term = this.state.term;
    const isFetching = this.state.isFetching;

    return (
      <PageContent title="Add New Movie">
        <PageContentBody>
          <div className={styles.searchContainer}>
            <div className={styles.searchIconContainer}>
              <Icon
                name={icons.SEARCH}
                size={20}
              />
            </div>

            <TextInput
              className={styles.searchInput}
              name="movieLookup"
              value={term}
              placeholder="eg. Brazzers, tmdb:####"
              autoFocus={true}
              onChange={this.onSearchInputChange}
            />

            <Button
              className={styles.clearLookupButton}
              onPress={this.onClearMovieLookupPress}
            >
              <Icon
                name={icons.REMOVE}
                size={20}
              />
            </Button>
          </div>

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error ?
              <div className={styles.message}>
                <div className={styles.helpText}>
                  Failed to load search results, please try again.
                </div>
                <div>{getErrorMessage(error)}</div>
              </div> : null
          }

          {
            !isFetching && !error && !!items.length &&
              <div className={styles.searchResults}>
                {
                  items.map((item) => {
                    return (
                      <AddNewMovieSearchResultConnector
                        key={item.tmdbId}
                        {...item}
                      />
                    );
                  })
                }
              </div>
          }

          {
            !isFetching && !error && !items.length && !!term &&
              <div className={styles.message}>
                <div className={styles.noResults}>Couldn't find any results for '{term}'</div>
                <div>You can also search using TMDB ID of a movie. eg. tmdb:123654</div>
                <div>
                  <Link to="https://wiki.servarr.com/whisparr/faq#why-cant-i-add-a-new-movie-when-i-know-the-tmdb-id">
                    Why can't I find my movie?
                  </Link>
                </div>
              </div>
          }

          {
            term ?
              null :
              <div className={styles.message}>
                <div className={styles.helpText}>
                  It's easy to add a new movie, just start typing the name the movie you want to add.
                </div>
                <div>You can also search using TheMovieDb ID of a movie. eg. tmdb:92</div>
              </div>
          }

          {
            !term && !hasExistingMovie ?
              <div className={styles.message}>
                <div className={styles.noMovieText}>
                  You haven't added any movies yet, do you want to import some or all of your movies first?
                </div>
                <div>
                  <Button
                    to="/add/import"
                    kind={kinds.PRIMARY}
                  >
                    Import Existing Movies
                  </Button>
                </div>
              </div> :
              null
          }

          <div />
        </PageContentBody>
      </PageContent>
    );
  }
}

AddNewMovie.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasExistingMovie: PropTypes.bool.isRequired,
  onMovieLookupChange: PropTypes.func.isRequired,
  onClearMovieLookup: PropTypes.func.isRequired
};

export default AddNewMovie;
