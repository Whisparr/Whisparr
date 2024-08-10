import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import PerformerDetailsLinks from 'Performer/Details/PerformerDetailsLinks';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import AddNewPerformerModal from './AddNewPerformerModal';
import styles from './AddNewPerformerSearchResult.css';

class AddNewPerformerSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddPerformerModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingPerformer && this.props.isExistingPerformer) {
      this.onAddPerformerModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddPerformerModalOpen: true });
  };

  onAddPerformerModalClose = () => {
    this.setState({ isNewAddPerformerModalOpen: false });
  };

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  };

  //
  // Render

  render() {
    const {
      foreignId,
      fullName,
      gender,
      status,
      images,
      isExistingPerformer,
      isSmallScreen,
      safeForWorkMode
    } = this.props;

    const {
      isNewAddPerformerModalOpen
    } = this.state;

    const linkProps = isExistingPerformer ? { to: `/performer/${foreignId}` } : { onPress: this.onPress };

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            isSmallScreen ?
              null :
              <div>
                <div className={styles.posterContainer}>
                  <MovieHeadshot
                    blur={safeForWorkMode}
                    className={styles.poster}
                    images={images}
                    size={250}
                    overflow={true}
                  />
                </div>
              </div>
          }

          <div className={styles.content}>
            <div className={styles.titleRow}>
              <div className={styles.titleContainer}>
                <div className={styles.title}>
                  {fullName}
                </div>
              </div>

              <div className={styles.icons}>

                {
                  isExistingPerformer &&
                    <Icon
                      className={styles.alreadyExistsIcon}
                      name={icons.CHECK_CIRCLE}
                      size={36}
                      title={translate('AlreadyInYourLibrary')}
                    />
                }
              </div>
            </div>

            <div>
              <Label size={sizes.LARGE} kind={kinds.PINK}>
                {translate('Performer')}
              </Label>

              {
                !!gender &&
                  <Label size={sizes.LARGE} kind={kinds.DEFAULT}>
                    {firstCharToUpper(gender)}
                  </Label>
              }

              <Tooltip
                anchor={
                  <Label
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.EXTERNAL_LINK}
                      size={13}
                    />

                    <span className={styles.links}>
                      Links
                    </span>
                  </Label>
                }
                tooltip={
                  <PerformerDetailsLinks
                    foreignId={foreignId}
                  />
                }
                canFlip={true}
                kind={kinds.INVERSE}
                position={tooltipPositions.BOTTOM}
              />

              {
                status === 'inactive' &&
                  <Label size={sizes.LARGE} kind={kinds.DANGER}>
                    {firstCharToUpper(status)}
                  </Label>
              }
            </div>
          </div>
        </div>

        <AddNewPerformerModal
          isOpen={isNewAddPerformerModalOpen && !isExistingPerformer}
          foreignId={foreignId}
          fullName={fullName}
          images={images}
          onModalClose={this.onAddPerformerModalClose}
        />
      </div>
    );
  }
}

AddNewPerformerSearchResult.propTypes = {
  foreignId: PropTypes.string.isRequired,
  fullName: PropTypes.string.isRequired,
  gender: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  existingPerformerId: PropTypes.number,
  isExistingPerformer: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  id: PropTypes.number,
  queueItems: PropTypes.arrayOf(PropTypes.object),
  monitored: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool,
  safeForWorkMode: PropTypes.bool
};

export default AddNewPerformerSearchResult;
