import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import StudioDetailsLinks from 'Studio/Details/StudioDetailsLinks';
import StudioLogo from 'Studio/StudioLogo';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import AddNewStudioModal from './AddNewStudioModal';
import styles from './AddNewStudioSearchResult.css';

class AddNewStudioSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddStudioModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingStudio && this.props.isExistingStudio) {
      this.onAddStudioModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddStudioModalOpen: true });
  };

  onAddStudioModalClose = () => {
    this.setState({ isNewAddStudioModalOpen: false });
  };

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  };

  //
  // Render

  render() {
    const {
      foreignId,
      title,
      network,
      status,
      images,
      isExistingStudio,
      isSmallScreen,
      safeForWorkMode
    } = this.props;

    const {
      isNewAddStudioModalOpen
    } = this.state;

    const linkProps = isExistingStudio ? { to: `/studio/${foreignId}` } : { onPress: this.onPress };

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
                  <StudioLogo
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
                  {title}
                </div>
              </div>

              <div className={styles.icons}>

                {
                  isExistingStudio &&
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
                {translate('Studio')}
              </Label>

              {
                !!network &&
                  <Label size={sizes.LARGE} kind={kinds.DEFAULT}>
                    {firstCharToUpper(network)}
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
                  <StudioDetailsLinks
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

        <AddNewStudioModal
          isOpen={isNewAddStudioModalOpen && !isExistingStudio}
          foreignId={foreignId}
          title={title}
          images={images}
          onModalClose={this.onAddStudioModalClose}
        />
      </div>
    );
  }
}

AddNewStudioSearchResult.propTypes = {
  foreignId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  network: PropTypes.string,
  status: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  existingStudioId: PropTypes.number,
  isExistingStudio: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  id: PropTypes.number,
  queueItems: PropTypes.arrayOf(PropTypes.object),
  monitored: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool,
  safeForWorkMode: PropTypes.bool
};

export default AddNewStudioSearchResult;
