import PropTypes from 'prop-types';
import React, { Component } from 'react';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import styles from './SelectMovieRow.css';

class SelectMovieRow extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onMovieSelect(this.props.id);
  };

  //
  // Render

  render() {
    const joinedPerformers = this.props.credits
      .slice(0, 5)
      .sort((a, b) => {
        return a.performer.name > b.performer.name ? 1 : -1;
      })
      .map((credit) => credit.performer.name)
      .join(', ');

    const {
      studioTitle,
      title,
      releaseDate
    } = this.props;
    return (
      <>
        <VirtualTableRowCell className={styles.studioTitle} title={studioTitle}>
          {studioTitle}
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.title} title={title}>
          {title}
        </VirtualTableRowCell>
        <VirtualTableRowCell className={styles.performers} title={joinedPerformers}>
          {joinedPerformers}
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.releaseDate}>
          <RelativeDateCellConnector
            key={name}
            date={releaseDate}
            className={styles.releaseDate}
          />
        </VirtualTableRowCell>

      </>
    );
  }
}

SelectMovieRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  studioTitle: PropTypes.string.isRequired,
  releaseDate: PropTypes.string.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  credits: PropTypes.array,
  onMovieSelect: PropTypes.func.isRequired
};

export default SelectMovieRow;
