import { useEffect, useState } from 'react';
import { invoke } from '@tauri-apps/api/core';
import {
  type Addon,
  type AccountInfo,
  type AppConfig,
  type Character,
  type CharacterEntry,
  type ConfigCharacter,
  type PushTarget,
} from './types';
import WtfFolderInput from './components/WtfFolderInput';
import PushAllBar from './components/PushAllBar';
import CharactersTable from './components/CharactersTable';
import AccountCharacterPanels from './components/AccountCharacterPanels';
import PushDialog from './components/PushDialog';
import ConfirmDialog from './components/ConfirmDialog';
import StatusBar from './components/StatusBar';
import './App.css';

function toCharacters(list: ConfigCharacter[]): Character[] {
  return list.map(c => ({ id: crypto.randomUUID(), ...c }));
}

function toConfigCharacters(list: Character[]): ConfigCharacter[] {
  return list.map(({ id: _id, ...rest }) => rest);
}

export default function App() {
  const [wtfPath, setWtfPath] = useState('');
  const [characters, setCharacters] = useState<Character[]>([]);
  const [accounts, setAccounts] = useState<AccountInfo[]>([]);
  const [isDirty, setIsDirty] = useState(false);
  const [lastPushStatus, setLastPushStatus] = useState<string | null>(null);
  const [pushTarget, setPushTarget] = useState<PushTarget | null>(null);
  const [confirmDialog, setConfirmDialog] = useState<{
    message: string;
    onConfirm: () => void;
  } | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const accountNames = accounts.map(a => a.name);

  // Load config on startup.
  useEffect(() => {
    (async () => {
      try {
        const config = await invoke<AppConfig>('get_config');
        setWtfPath(config.wtfPath);
        setCharacters(toCharacters(config.characters));
        if (config.wtfPath) await scanWtfFolder(config.wtfPath);
        const status = await invoke<string | null>('get_last_push_status');
        setLastPushStatus(status);
      } catch (e) {
        setErrorMessage(String(e));
      }
    })();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const scanWtfFolder = async (path: string) => {
    try {
      const result = await invoke<AccountInfo[]>('scan_wtf_folder', { wtfPath: path });
      setAccounts(result);
    } catch (e) {
      setErrorMessage(`Failed to scan WTF folder: ${e}`);
    }
  };

  const handleApply = async (path: string) => {
    try {
      const newConfig: AppConfig = { wtfPath: path, characters: toConfigCharacters(characters) };
      await invoke('save_config', { config: newConfig });
      await invoke('append_log', { keyword: 'ACTION', message: `Set WTF path: ${path}` });
      setWtfPath(path);
      setIsDirty(false);
      await scanWtfFolder(path);
    } catch (e) {
      setErrorMessage(String(e));
    }
  };

  const handleUpdateCharacter = (id: string, updates: Partial<Character>) => {
    setCharacters(prev => prev.map(c => (c.id === id ? { ...c, ...updates } : c)));
    setIsDirty(true);
  };

  const handleRemoveCharacter = (id: string) => {
    const char = characters.find(c => c.id === id);
    setConfirmDialog({
      message: `Remove "${char ? `${char.characterName}-${char.realmName}` : 'this character'}" from the list?`,
      onConfirm: () => {
        setCharacters(prev => prev.filter(c => c.id !== id));
        setIsDirty(true);
        setConfirmDialog(null);
        if (char) {
          invoke('append_log', {
            keyword: 'ACTION',
            message: `Removed character ${char.characterName}-${char.realmName}-${char.account}`,
          }).catch(() => {});
        }
      },
    });
  };

  const handleRemoveAll = () => {
    setConfirmDialog({
      message: 'Remove all characters from the list?',
      onConfirm: () => {
        setCharacters([]);
        setIsDirty(true);
        setConfirmDialog(null);
        invoke('append_log', { keyword: 'ACTION', message: 'Removed all characters' }).catch(() => {});
      },
    });
  };

  const handleAddCharacter = (char: CharacterEntry, account: string) => {
    const isDupe = characters.some(
      c => c.characterName === char.name && c.realmName === char.realm && c.account === account,
    );
    if (isDupe) return;
    const newChar: Character = {
      id: crypto.randomUUID(),
      characterName: char.name,
      realmName: char.realm,
      account,
      faction: 'Alliance',
      enabled: true,
    };
    setCharacters(prev => [...prev, newChar]);
    setIsDirty(true);
    invoke('append_log', {
      keyword: 'ACTION',
      message: `Added character ${char.name}-${char.realm}-${account}`,
    }).catch(() => {});
  };

  const handleSave = async () => {
    try {
      await invoke('save_config', {
        config: { wtfPath, characters: toConfigCharacters(characters) },
      });
      await invoke('append_log', { keyword: 'ACTION', message: 'Saved character list' });
      setIsDirty(false);
    } catch (e) {
      setErrorMessage(String(e));
    }
  };

  const openPushDialog = (addon: Addon, char: Character | null) => {
    if (char) {
      setPushTarget({
        addon,
        character: `${char.characterName}-${char.realmName}-${char.account}`,
        displayName: `${char.characterName}-${char.realmName}`,
      });
    } else {
      setPushTarget({ addon, character: 'all', displayName: 'All Characters' });
    }
  };

  const handlePushComplete = async () => {
    const status = await invoke<string | null>('get_last_push_status').catch(() => null);
    setLastPushStatus(status);
  };

  return (
    <div className="app">
      <WtfFolderInput wtfPath={wtfPath} onApply={handleApply} />
      <PushAllBar onPushAll={addon => openPushDialog(addon, null)} />

      <div className="table-section">
        <div className="table-scroll">
          <CharactersTable
            characters={characters}
            accounts={accountNames}
            onUpdate={handleUpdateCharacter}
            onRemove={handleRemoveCharacter}
            onPush={(char, addon) => openPushDialog(addon, char)}
          />
        </div>
        <div className="table-footer">
          <button
            className="btn btn-danger-outline"
            onClick={handleRemoveAll}
            disabled={characters.length === 0}
          >
            <span>✕</span> Remove All
          </button>
          <button className="btn btn-primary" onClick={handleSave} disabled={!isDirty}>
            Save
          </button>
        </div>
      </div>

      <AccountCharacterPanels accounts={accounts} onAdd={handleAddCharacter} />
      <StatusBar lastPushStatus={lastPushStatus} />

      {pushTarget && (
        <PushDialog
          target={pushTarget}
          onClose={() => setPushTarget(null)}
          onPushComplete={handlePushComplete}
        />
      )}

      {confirmDialog && (
        <ConfirmDialog
          message={confirmDialog.message}
          onConfirm={confirmDialog.onConfirm}
          onCancel={() => setConfirmDialog(null)}
        />
      )}

      {errorMessage && (
        <div className="error-banner" onClick={() => setErrorMessage(null)}>
          ⚠ {errorMessage} — click to dismiss
        </div>
      )}
    </div>
  );
}
