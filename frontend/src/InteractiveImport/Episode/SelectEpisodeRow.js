import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';

class SelectEpisodeRow extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      isSelected
    } = this.props;

    this.props.onSelectedChange({ id, value: !isSelected });
  };

  //
  // Render

  render() {
    const {
      id,
      title,
      releaseDate,
      actors,
      isSelected,
      onSelectedChange
    } = this.props;

    const joinedPerformers = actors.map((a) => a.character).slice(0, 4).join(', ');

    return (
      <TableRowButton onPress={this.onPress}>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell>
          {title}
        </TableRowCell>

        <TableRowCell>
          <span title={joinedPerformers}>
            {joinedPerformers}
          </span>
        </TableRowCell>

        <TableRowCell>
          {releaseDate}
        </TableRowCell>
      </TableRowButton>
    );
  }
}

SelectEpisodeRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  actors: PropTypes.arrayOf(PropTypes.object).isRequired,
  releaseDate: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectEpisodeRow;
