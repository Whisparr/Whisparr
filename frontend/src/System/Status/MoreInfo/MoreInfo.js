import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';

class MoreInfo extends Component {

  //
  // Render

  render() {
    return (
      <FieldSet legend={translate('MoreInfo')}>
        <DescriptionList>
          <DescriptionListItemTitle>
            {translate('HomePage')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://whisparr.com/">whisparr.com</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Wiki')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://wiki.servarr.com/whisparr">{translate('Wiki')}</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Reddit')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://www.reddit.com/r/Whisparr/">/r/Whisparr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Discord')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://whisparr.com/discord">whisparr.com/discord</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Source')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Whisparr/Whisparr/">github.com/Whisparr/Whisparr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('FeatureRequests')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Whisparr/Whisparr/issues">github.com/Whisparr/Whisparr/issues</Link>
          </DescriptionListItemDescription>

        </DescriptionList>
      </FieldSet>
    );
  }
}

MoreInfo.propTypes = {

};

export default MoreInfo;
