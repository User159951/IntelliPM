import { FixedSizeList as List } from 'react-window';
import { useMemo } from 'react';

interface VirtualizedListProps<T> {
  items: T[];
  renderItem: (item: T, index: number) => React.ReactNode;
  itemHeight?: number;
  maxHeight?: number;
  className?: string;
}

/**
 * Virtualized list component for rendering large lists efficiently.
 * Only renders visible items, improving performance for long AI result lists.
 */
export function VirtualizedList<T>({
  items,
  renderItem,
  itemHeight = 60,
  maxHeight = 400,
  className,
}: VirtualizedListProps<T>) {
  const Row = useMemo(
    () =>
      ({ index, style }: { index: number; style: React.CSSProperties }) => {
        const item = items[index];
        return (
          <div style={style} className="px-2">
            {renderItem(item, index)}
          </div>
        );
      },
    [items, renderItem]
  );

  if (items.length === 0) {
    return (
      <div className="text-center text-muted-foreground py-8">
        No items to display
      </div>
    );
  }

  // For small lists, render normally without virtualization
  if (items.length <= 10) {
    return (
      <div className={className}>
        {items.map((item, index) => (
          <div key={index} className="mb-2">
            {renderItem(item, index)}
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className={className}>
      <List
        height={Math.min(maxHeight, items.length * itemHeight)}
        itemCount={items.length}
        itemSize={itemHeight}
        width="100%"
      >
        {Row}
      </List>
    </div>
  );
}

