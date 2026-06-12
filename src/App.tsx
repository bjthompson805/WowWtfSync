import { useEffect, useMemo, useRef, useState } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { getCurrentWindow } from '@tauri-apps/api/window';
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
import UnsavedChangesDialog from './components/UnsavedChangesDialog';
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
  const [savedCharacters, setSavedCharacters] = useState<ConfigCharacter[]>([]);
  const isDirty = useMemo(
    () => JSON.stringify(toConfigCharacters(characters)) !== JSON.stringify(savedCharacters),
    [characters, savedCharacters],
  );
  const [lastPushStatus, setLastPushStatus] = useState<string | null>(null);
  const [pushTarget, setPushTarget] = useState<PushTarget | null>(null);
  const [confirmDialog, setConfirmDialog] = useState<{
    message: string;
    onConfirm: () => void;
  } | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showExitDialog, setShowExitDialog] = useState(false);
  const [pendingPushArgs, setPendingPushArgs] = useState<{
    addon: Addon | 'all';
    char: Character | null;
  } | null>(null);

  // Ref so the close-event listener always sees the latest isDirty value.
  const isDirtyRef = useRef(isDirty);
  useEffect(() => { isDirtyRef.current = isDirty; }, [isDirty]);

  const accountNames = accounts.map(a => a.name);

  // Load config on startup.
  useEffect(() => {
    (async () => {
      try {
        const config = await invoke<AppConfig>('get_config');
        setWtfPath(config.wtfPath);
        setCharacters(toCharacters(config.characters));
        setSavedCharacters(config.characters);
        if (config.wtfPath) await scanWtfFolder(config.wtfPath);
        const status = await invoke<string | null>('get_last_push_status');
        setLastPushStatus(status);
      } catch (e) {
        setErrorMessage(String(e));
      }
    })();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Intercept window close. In Tauri v2, registering any onCloseRequested listener
  // means the window will NOT close unless we explicitly call .close() ourselves.
  useEffect(() => {
    let unlisten: (() => void) | undefined;
    const win = getCurrentWindow();
    win.onCloseRequested(async (event) => {
      event.preventDefault();
      if (isDirtyRef.current) {
        setShowExitDialog(true);
      } else {
        await win.destroy();
      }
    }).then(fn => { unlisten = fn; });
    return () => { unlisten?.(); };
  }, []);

  const handleSaveAndExit = async () => {
    try {
      await invoke('save_config', { config: { wtfPath, characters: toConfigCharacters(characters) } });
      await invoke('append_log', { keyword: 'ACTION', message: 'Saved character list on exit' });
      await getCurrentWindow().destroy();
    } catch (e) {
      setShowExitDialog(false);
      setErrorMessage(String(e));
    }
  };

  const handleExitWithoutSaving = async () => {
    await getCurrentWindow().destroy();
  };

  // Opens the push dialog, but first prompts if there are unsaved changes.
  const requestPush = (addon: Addon | 'all', char: Character | null) => {
    if (isDirty) {
      setPendingPushArgs({ addon, char });
    } else {
      openPushDialog(addon, char);
    }
  };

  const handleSaveAndContinuePush = async () => {
    if (!pendingPushArgs) return;
    try {
      await invoke('save_config', { config: { wtfPath, characters: toConfigCharacters(characters) } });
      await invoke('append_log', { keyword: 'ACTION', message: 'Saved character list before push' });
      setSavedCharacters(toConfigCharacters(characters));
      openPushDialog(pendingPushArgs.addon, pendingPushArgs.char);
    } catch (e) {
      setErrorMessage(String(e));
    } finally {
      setPendingPushArgs(null);
    }
  };

  const handleContinuePushWithoutSaving = () => {
    if (!pendingPushArgs) return;
    openPushDialog(pendingPushArgs.addon, pendingPushArgs.char);
    setPendingPushArgs(null);
  };

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
      setSavedCharacters(toConfigCharacters(characters));
      await scanWtfFolder(path);
    } catch (e) {
      setErrorMessage(String(e));
    }
  };

  const handleUpdateCharacter = (id: string, updates: Partial<Character>) => {
    setCharacters(prev => prev.map(c => (c.id === id ? { ...c, ...updates } : c)));
  };

  const handleRemoveCharacter = (id: string) => {
    const char = characters.find(c => c.id === id);
    setConfirmDialog({
      message: `Remove "${char ? `${char.characterName}-${char.realmName}` : 'this character'}" from the list?`,
      onConfirm: () => {
        setCharacters(prev => prev.filter(c => c.id !== id));
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
      setSavedCharacters(toConfigCharacters(characters));
    } catch (e) {
      setErrorMessage(String(e));
    }
  };

  const openPushDialog = (addon: Addon | 'all', char: Character | null) => {
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
      <PushAllBar onPushAll={addon => requestPush(addon, null)} />

      <div className="table-section">
        <div className="table-scroll">
          <CharactersTable
            characters={characters}
            accounts={accountNames}
            onUpdate={handleUpdateCharacter}
            onRemove={handleRemoveCharacter}
            onPush={(char, addon) => requestPush(addon, char)}
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

      {showExitDialog && (
        <UnsavedChangesDialog
          title="You have unsaved changes. What would you like to do?"
          primaryLabel="Save and Exit"
          secondaryLabel="Exit Without Saving"
          onPrimary={handleSaveAndExit}
          onSecondary={handleExitWithoutSaving}
          onCancel={() => setShowExitDialog(false)}
        />
      )}

      {pendingPushArgs && (
        <UnsavedChangesDialog
          title="You have unsaved changes. What would you like to do?"
          primaryLabel="Save and Continue"
          secondaryLabel="Continue Without Saving"
          onPrimary={handleSaveAndContinuePush}
          onSecondary={handleContinuePushWithoutSaving}
          onCancel={() => setPendingPushArgs(null)}
        />
      )}
    </div>
  );
}
