import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';

function SceneFormats({ formats }) {
  return (
    <div>
      {
        formats.map((format) => {
          return (
            <Label
              key={format.id}
              kind={kinds.INFO}
            >
              {format.name}
            </Label>
          );
        })
      }
    </div>
  );
}

SceneFormats.propTypes = {
  formats: PropTypes.arrayOf(PropTypes.object).isRequired
};

SceneFormats.defaultProps = {
  formats: []
};

export default SceneFormats;
