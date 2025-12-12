import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';
import { icons } from 'Helpers/Props';

function SelectPerformerSitesRow(props) {
  const {
    id,
    title,
    year,
    sceneCount,
    exists,
    isSelected,
    onSelectedChange
  } = props;

  const onPress = useCallback(() => {
    onSelectedChange({ id, value: !isSelected });
  }, [id, isSelected, onSelectedChange]);

  return (
    <TableRowButton onPress={onPress}>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      <TableRowCell>
        {title}
      </TableRowCell>

      <TableRowCell>
        {year > 0 ? year : '-'}
      </TableRowCell>

      <TableRowCell>
        {sceneCount}
      </TableRowCell>

      <TableRowCell>
        {exists ? (
          <Icon
            name={icons.CHECK}
            kind="success"
            title="In Library"
          />
        ) : null}
      </TableRowCell>
    </TableRowButton>
  );
}

SelectPerformerSitesRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  sceneCount: PropTypes.number.isRequired,
  exists: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectPerformerSitesRow;
