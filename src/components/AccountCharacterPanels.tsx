import { useState } from 'react';
import type { AccountInfo, CharacterEntry } from '../types';

interface Props {
  accounts: AccountInfo[];
  onAdd: (char: CharacterEntry, account: string) => void;
}

export default function AccountCharacterPanels({ accounts, onAdd }: Props) {
  const [selectedAccount, setSelectedAccount] = useState<string | null>(null);
  const [selectedChar, setSelectedChar] = useState<CharacterEntry | null>(null);

  const currentAccount = accounts.find(a => a.name === selectedAccount) ?? null;

  const handleSelectAccount = (name: string) => {
    setSelectedAccount(name);
    setSelectedChar(null);
  };

  const handleAdd = () => {
    if (selectedChar && selectedAccount) {
      onAdd(selectedChar, selectedAccount);
    }
  };

  return (
    <div className="browser-section">
      <div className="browser-panels">
        {/* Account list */}
        <div className="browser-panel">
          <div className="panel-header">Accounts</div>
          <div className="panel-list">
            {accounts.length === 0 ? (
              <div className="panel-empty">Apply a WTF folder path to scan for accounts.</div>
            ) : (
              accounts.map(account => (
                <div
                  key={account.name}
                  className={`panel-item ${selectedAccount === account.name ? 'selected' : ''}`}
                  onClick={() => handleSelectAccount(account.name)}
                >
                  {account.name}
                </div>
              ))
            )}
          </div>
        </div>

        {/* Character list */}
        <div className="browser-panel">
          <div className="panel-header">Characters</div>
          <div className="panel-list">
            {!currentAccount ? (
              <div className="panel-empty">Select an account to view characters.</div>
            ) : currentAccount.characters.length === 0 ? (
              <div className="panel-empty">No characters found in this account.</div>
            ) : (
              [...currentAccount.characters]
                .sort((a, b) => `${a.name}-${a.realm}`.localeCompare(`${b.name}-${b.realm}`))
                .map(char => {
                const key = `${char.name}-${char.realm}`;
                const isSelected =
                  selectedChar?.name === char.name && selectedChar?.realm === char.realm;
                return (
                  <div
                    key={key}
                    className={`panel-item ${isSelected ? 'selected' : ''}`}
                    onClick={() => setSelectedChar(char)}
                  >
                    {char.name}-{char.realm}
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

      <div className="browser-footer">
        <button
          className="btn btn-primary"
          disabled={!selectedChar}
          onClick={handleAdd}
        >
          Add
        </button>
      </div>
    </div>
  );
}
