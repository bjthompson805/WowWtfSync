import { useState } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { ADDON_DESCRIPTIONS, type PushTarget } from '../types';

interface Props {
  target: PushTarget;
  onClose: () => void;
  onPushComplete: (logMessage: string) => void;
}

type Phase = 'confirm' | 'pushing' | 'done' | 'error';

function buildDescription(target: PushTarget): string {
  const { addon, character, displayName } = target;
  const isAll = character === 'all';

  if (addon === 'Altoholic') {
    return isAll
      ? 'Push Altoholic data from all accounts, combining the data.'
      : `Push Altoholic data for ${displayName} by combining the data to/from all accounts.`;
  }

  const who = isAll ? 'each character' : displayName;
  const addonDesc = ADDON_DESCRIPTIONS[addon] ?? '';
  return `Push ${addon} data from ${who} to all accounts that are not the same as the character's account. ${addonDesc}`;
}

export default function PushDialog({ target, onClose, onPushComplete }: Props) {
  const [phase, setPhase] = useState<Phase>('confirm');
  const [resultMessage, setResultMessage] = useState('');
  const { addon, character, displayName } = target;
  const isAll = character === 'all';

  const headerLabel = isAll ? 'All Characters' : displayName;
  const description = buildDescription(target);

  const handlePush = async () => {
    setPhase('pushing');
    try {
      const result = await invoke<string>('run_push', { addon: addon.toLowerCase(), character });
      const logMsg = `Pushed ${addon} for ${isAll ? 'all characters' : displayName}`;
      await invoke('append_log', { keyword: 'PUSH', message: logMsg });
      onPushComplete(logMsg);
      setResultMessage(result.trim());
      setPhase('done');
    } catch (err) {
      setResultMessage(String(err));
      setPhase('error');
    }
  };

  return (
    <div className="dialog-backdrop">
      <div className="dialog push-dialog">
        <h2 className="push-dialog-header">
          Push {addon} — {headerLabel}
        </h2>

        {(phase === 'confirm' || phase === 'pushing') && (
          <>
            <p className="push-dialog-desc">{description}</p>
            <p className="push-dialog-warning">
              Make sure to log out of all characters that are being pushed to,
              otherwise the data will be overwritten when logout occurs.
            </p>
            {addon === 'Altoholic' && (
              <p className="push-dialog-note">
                *Aggregation of mail sent to your own character from another account is also supported.
              </p>
            )}
          </>
        )}

        {phase === 'done' && (
          <div className="push-result success">
            <pre>{resultMessage}</pre>
          </div>
        )}

        {phase === 'error' && (
          <div className="push-result error">
            <p className="push-error-label">An error occurred:</p>
            <pre>{resultMessage}</pre>
          </div>
        )}

        <div className="dialog-footer">
          {(phase === 'confirm' || phase === 'pushing') ? (
            <button
              className="btn btn-primary btn-large"
              onClick={handlePush}
              disabled={phase === 'pushing'}
            >
              {phase === 'pushing' ? 'Pushing…' : 'Push'}
            </button>
          ) : (
            <button className="btn btn-primary" onClick={onClose}>OK</button>
          )}
          {phase === 'confirm' && (
            <button className="btn" onClick={onClose}>Cancel</button>
          )}
        </div>
      </div>
    </div>
  );
}
