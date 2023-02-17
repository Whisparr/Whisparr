import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function SeriesMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="All Scenes"
        data="Monitor all scenes except specials"
      />

      <DescriptionListItem
        title="Future Scenes"
        data="Monitor scenes that have not aired yet"
      />

      <DescriptionListItem
        title="Missing Scenes"
        data="Monitor scenes that do not have files or have not aired yet"
      />

      <DescriptionListItem
        title="Existing Scenes"
        data="Monitor scenes that have files or have not aired yet"
      />

      <DescriptionListItem
        title="First Year"
        data="Monitor all scenes of the first year. All other seasons will be ignored"
      />

      <DescriptionListItem
        title="Latest Year"
        data="Monitor all scenes of the latest year and future seasons"
      />

      <DescriptionListItem
        title="None"
        data="No scenes will be monitored"
      />
    </DescriptionList>
  );
}

export default SeriesMonitoringOptionsPopoverContent;
