import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';

class MoreInfo extends Component {

  //
  // Render

  render() {
    return (
      <FieldSet legend="More Info">
        <DescriptionList>
          <DescriptionListItemTitle>Home page</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://whisparr.tv/">whisparr.tv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Wiki</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://wiki.servarr.com/whisparr">wiki.servarr.com/whisparr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Forums</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://forums.whisparr.tv/">forums.whisparr.tv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Twitter</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://twitter.com/whisparrtv">@whisparrtv</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Discord</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://discord.gg/73QUuf3bgA">discord.gg/73QUuf3bgA</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>IRC</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="irc://irc.libera.chat/#whisparr">#whisparr on Libera</Link>
          </DescriptionListItemDescription>
          <DescriptionListItemDescription>
            <Link to="https://web.libera.chat/?channels=#whisparr">Libera webchat</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Donations</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://whisparr.tv/donate">whisparr.tv/donate</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Source</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Whisparr/Whisparr/">github.com/Whisparr/Whisparr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Feature Requests</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://forums.whisparr.tv/">forums.whisparr.tv</Link>
          </DescriptionListItemDescription>
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
