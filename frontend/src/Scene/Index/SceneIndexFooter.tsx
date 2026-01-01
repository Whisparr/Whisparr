import classNames from 'classnames';
import React from 'react';
import { useSelector } from 'react-redux';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import AppState from 'App/State/AppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './SceneIndexFooter.css';

function createSceneSelector() {
  return createDeepEqualSelector(
    (state: AppState) => state.scenePages.items ?? [],
    (scenes) =>
      scenes.map((scene) => {
        const { monitored, status, hasFile, statistics } = scene;

        return {
          monitored,
          status,
          hasFile,
          statistics,
        };
      })
  );
}

export default function SceneIndexFooter() {
  const scenes = useSelector(createSceneSelector());
  const count = scenes.length;
  let sceneFiles = 0;
  let monitored = 0;
  let totalFileSize = 0;

  scenes.forEach((s) => {
    const { statistics = { sizeOnDisk: 0 } } = s;

    const { sizeOnDisk = 0 } = statistics;

    if (s.hasFile) {
      sceneFiles += 1;
    }

    if (s.monitored) {
      monitored++;
    }

    totalFileSize += sizeOnDisk;
  });

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div className={styles.footer}>
            <div>
              <div className={styles.legendItem}>
                <div className={styles.ended} />
                <div>{translate('DownloadedAndMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.availNotMonitored} />
                <div>{translate('DownloadedButNotMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingMonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('MissingMonitoredAndConsideredAvailable')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingUnmonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('MissingNotMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.queue} />
                <div>{translate('Queued')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.continuing} />
                <div>{translate('Unreleased')}</div>
              </div>
            </div>

            <div className={styles.statistics}>
              <DescriptionList>
                <DescriptionListItem title={translate('Scenes')} data={count} />

                <DescriptionListItem
                  title={translate('SceneFiles')}
                  data={sceneFiles}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('Monitored')}
                  data={monitored}
                />

                <DescriptionListItem
                  title={translate('Unmonitored')}
                  data={count - monitored}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('TotalFileSize')}
                  data={formatBytes(totalFileSize)}
                />
              </DescriptionList>
            </div>
          </div>
        );
      }}
    </ColorImpairedConsumer>
  );
}
