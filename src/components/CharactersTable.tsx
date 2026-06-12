import { useEffect, useRef, useState } from 'react';
import { X, ArrowUp, ArrowUpDown, ArrowUpNarrowWide, ArrowDownNarrowWide } from 'lucide-react';
import { ADDONS, type Addon, type Character, type Faction } from '../types';

type AddonOrAll = Addon | 'all';

interface Props {
  characters: Character[];
  accounts: string[];
  onUpdate: (id: string, updates: Partial<Character>) => void;
  onRemove: (id: string) => void;
  onPush: (character: Character, addon: AddonOrAll) => void;
}

type SortDir = 'asc' | 'desc';
type SortCol = 'character' | 'account' | 'faction' | 'enabled';

const COL_ACTIONS = 0;
const COL_CHARACTER = 1;
const COL_ACCOUNT = 2;
const COL_FACTION = 3;
const COL_ENABLED = 4;

const MIN_WIDTHS = [72, 100, 100, 90, 72];
const INIT_WIDTHS = [72, 220, 200, 120, 80];

export default function CharactersTable({
  characters,
  accounts,
  onUpdate,
  onRemove,
  onPush,
}: Props) {
  const [widths, setWidths] = useState<number[]>(INIT_WIDTHS);
  const [sortCol, setSortCol] = useState<SortCol | null>(null);
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [contextMenu, setContextMenu] = useState<{
    charId: string;
    x: number;
    y: number;
  } | null>(null);

  const tableRef = useRef<HTMLTableElement>(null);
  const resizeRef = useRef<{
    col: number;
    startX: number;
    startWidths: number[];
    scale: number;
  } | null>(null);

  // Close context menu on outside click.
  useEffect(() => {
    const handler = () => setContextMenu(null);
    window.addEventListener('click', handler);
    return () => window.removeEventListener('click', handler);
  }, []);

  // Column resize via mouse drag.
  useEffect(() => {
    const onMouseMove = (e: MouseEvent) => {
      if (!resizeRef.current) return;
      const { col, startX, startWidths, scale } = resizeRef.current;
      const delta = (e.clientX - startX) / scale;
      const next = col + 1;

      const newCurrent = Math.max(startWidths[col] + delta, MIN_WIDTHS[col]);
      const actualDelta = newCurrent - startWidths[col];
      const newNext = Math.max(startWidths[next] - actualDelta, MIN_WIDTHS[next]);

      setWidths(prev => {
        const updated = [...prev];
        updated[col] = newCurrent;
        updated[next] = newNext;
        return updated;
      });
    };

    const onMouseUp = () => {
      resizeRef.current = null;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
    };

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
    };
  }, []);

  const startResize = (col: number, e: React.MouseEvent) => {
    e.preventDefault();
    const tableWidth = tableRef.current?.offsetWidth ?? widths.reduce((a, b) => a + b, 0);
    const totalPixelWidth = widths.reduce((a, b) => a + b, 0);
    const scale = tableWidth / totalPixelWidth;
    resizeRef.current = { col, startX: e.clientX, startWidths: [...widths], scale };
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  };

  const handleSortClick = (col: SortCol) => {
    if (sortCol === col) {
      if (sortDir === 'asc') setSortDir('desc');
      else if (sortDir === 'desc') { setSortCol(null); }
    } else {
      setSortCol(col);
      setSortDir('asc');
    }
  };

  const sortedChars = [...characters].sort((a, b) => {
    if (!sortCol) return 0;
    let aVal = '';
    let bVal = '';
    if (sortCol === 'character') { aVal = `${a.characterName}-${a.realmName}`; bVal = `${b.characterName}-${b.realmName}`; }
    if (sortCol === 'account') { aVal = a.account; bVal = b.account; }
    if (sortCol === 'faction') { aVal = a.faction; bVal = b.faction; }
    if (sortCol === 'enabled') { aVal = a.enabled ? '1' : '0'; bVal = b.enabled ? '1' : '0'; }
    const cmp = aVal.localeCompare(bVal);
    return sortDir === 'asc' ? cmp : -cmp;
  });

  const SortIcon = ({ col }: { col: SortCol }) => {
    if (sortCol !== col) return <ArrowUpDown size={12} className="sort-icon inactive" />;
    if (sortDir === 'asc') return <ArrowUpNarrowWide size={12} className="sort-icon" />;
    return <ArrowDownNarrowWide size={12} className="sort-icon" />;
  };

  const handlePushButtonClick = (char: Character, e: React.MouseEvent) => {
    e.stopPropagation();
    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    setContextMenu({ charId: char.id, x: rect.left, y: rect.bottom + 4 });
  };

  const handleContextAddon = (addon: AddonOrAll) => {
    if (!contextMenu) return;
    const char = characters.find(c => c.id === contextMenu.charId);
    if (char) onPush(char, addon);
    setContextMenu(null);
  };

  return (
    <div className="characters-table-wrapper">
      <table className="characters-table" ref={tableRef}>
        <colgroup>
          {widths.map((w, i) => <col key={i} style={{ width: w }} />)}
        </colgroup>
        <thead>
          <tr>
            <th className="th-actions" />
            {(['character', 'account', 'faction', 'enabled'] as SortCol[]).map((col, idx) => {
              const colIdx = idx + 1;
              const label = col.charAt(0).toUpperCase() + col.slice(1);
              // Only Character (1) and Account (2) have resize handles.
              const hasResizeHandle = colIdx === COL_CHARACTER || colIdx === COL_ACCOUNT;
              return (
                <th key={col} className="th-sortable">
                  <span className="th-content" onClick={() => handleSortClick(col)}>
                    {label}
                    <SortIcon col={col} />
                  </span>
                  {hasResizeHandle && (
                    <span
                      className="resize-handle"
                      onMouseDown={e => startResize(colIdx, e)}
                    />
                  )}
                </th>
              );
            })}
          </tr>
        </thead>
        <tbody>
          {sortedChars.length === 0 ? (
            <tr>
              <td colSpan={5} className="table-empty">
                No characters added yet. To get started:
                {' '}enter the path to your WoW <strong>WTF</strong> folder above and click <strong>Apply</strong>,
                {' '}then select an account on the left below, click a character on the right, and click <strong>Add</strong>.
              </td>
            </tr>
          ) : (
            sortedChars.map(char => (
              <tr key={char.id}>
                {/* Action buttons */}
                <td className="td-actions">
                  <span className="td-actions-inner">
                    <button
                      className="action-btn remove-btn"
                      title="Remove character"
                      onClick={() => onRemove(char.id)}
                    >
                      <X size={12} />
                    </button>
                    <button
                      className="action-btn push-btn"
                      title="Push this character's addon data"
                      onClick={e => handlePushButtonClick(char, e)}
                    >
                      <ArrowUp size={12} />
                    </button>
                  </span>
                </td>

                {/* Character name */}
                <td className="td-text">
                  {char.characterName}-{char.realmName}
                </td>

                {/* Account dropdown */}
                <td className="td-select">
                  <select
                    value={char.account}
                    onChange={e => onUpdate(char.id, { account: e.target.value })}
                  >
                    {accounts.map(acc => (
                      <option key={acc} value={acc}>{acc}</option>
                    ))}
                    {/* Keep the saved value visible even if accounts haven't been scanned yet */}
                    {!accounts.includes(char.account) && (
                      <option value={char.account}>{char.account}</option>
                    )}
                  </select>
                </td>

                {/* Faction dropdown */}
                <td className="td-select">
                  <select
                    value={char.faction}
                    onChange={e => onUpdate(char.id, { faction: e.target.value as Faction })}
                  >
                    <option value="Alliance">Alliance</option>
                    <option value="Horde">Horde</option>
                  </select>
                </td>

                {/* Enabled checkbox */}
                <td className="td-center">
                  <input
                    type="checkbox"
                    checked={char.enabled}
                    onChange={e => onUpdate(char.id, { enabled: e.target.checked })}
                  />
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      {/* Per-character push context menu */}
      {contextMenu && (
        <div
          className="context-menu"
          style={{ top: contextMenu.y, left: contextMenu.x }}
          onClick={e => e.stopPropagation()}
        >
          <div className="context-menu-item" onClick={() => handleContextAddon('all')}>
            <ArrowUp size={12} />
            ALL
          </div>
          <div className="context-menu-separator" />
          {ADDONS.map(addon => (
            <div
              key={addon}
              className="context-menu-item"
              onClick={() => handleContextAddon(addon)}
            >
              <ArrowUp size={12} />
              {addon}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
