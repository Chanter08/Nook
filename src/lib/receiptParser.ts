export interface ParsedReceiptItem {
  id: string;
  name: string;
  quantity: number;
}

const ignoredLine = /^(total|sub\s*total|vat|tax|cash|change|card|visa|mastercard|payment|amount|balance|receipt|thank|date|time|tel|telephone|www\.|invoice|eur\b|€)/i;
const trailingPrice = /\s+(?:€\s*)?\d+[.,]\d{2}\s*$/;

export function parseReceiptText(text: string): ParsedReceiptItem[] {
  const candidates: ParsedReceiptItem[] = [];

  for (const rawLine of text.split(/\r?\n/)) {
    let line = rawLine.replace(/\s+/g, " ").trim();

    if (!line || line.length < 3 || ignoredLine.test(line) || !/[A-Za-z]/.test(line) || !trailingPrice.test(line)) {
      continue;
    }

    line = line.replace(trailingPrice, "").trim();
    line = line.replace(/^\d{5,}\s+/, "");

    let quantity = 1;
    const quantityMatch = line.match(/^(\d+(?:[.,]\d+)?)\s*[xX]\s+(.+)$/);

    if (quantityMatch) {
      quantity = Number(quantityMatch[1].replace(",", ".")) || 1;
      line = quantityMatch[2].trim();
    }

    line = line.replace(/^[*#-]+\s*/, "").replace(/\s{2,}/g, " ").trim();

    if (line.length < 2 || ignoredLine.test(line)) {
      continue;
    }

    candidates.push({
      id: crypto.randomUUID(),
      name: line,
      quantity
    });
  }

  return candidates.slice(0, 100);
}
