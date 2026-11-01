import { test, expect } from '@playwright/test';

test('manager approval limit is enforced for an over-limit invoice', async ({ page }, testInfo) => {
  await page.goto('/');

  await page.getByLabel('Username').fill('demo.manager');
  await page.getByLabel('Password').fill('ChangeMe123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByText('demo.manager (Manager)')).toBeVisible();

  const customerName = `Atlas Labs ${Date.now()}`;
  await page.getByLabel('Customer name').fill(customerName);
  await page.getByLabel('Amount').fill('60000');
  await page.getByRole('button', { name: 'Create invoice' }).click();

  const invoiceRow = page.locator('#invoices-table tbody tr').filter({ hasText: customerName }).first();
  await expect(invoiceRow).toContainText(customerName);

  const invoiceId = (await invoiceRow.locator('td').first().textContent())?.trim();
  expect(invoiceId).toBeTruthy();

  await invoiceRow.getByRole('button', { name: /Submit invoice .* for approval/i }).click();

  await page.getByRole('tab', { name: 'Approvals' }).click();

  const pendingInvoice = page.locator('#pending-list li').filter({ hasText: customerName }).first();
  await expect(pendingInvoice).toContainText(customerName);

  const approveButton = page.getByRole('button', { name: new RegExp(`Approve invoice ${invoiceId}`, 'i') });
  await approveButton.click();

  const errorMessage = page.locator('#decision-error');
  await expect(errorMessage).toBeVisible();
  await expect(errorMessage).toContainText(/manager.*50,000|exceeds the maximum|exceeds.*50,000/i);

  await page.screenshot({ path: testInfo.outputPath('approval-limit-rejected.png'), fullPage: true });

  const latestInvoice = await page.evaluate(async (id) => {
    const session = JSON.parse(sessionStorage.getItem('session') || 'null');
    const response = await fetch(`/api/invoices/${id}`, {
      headers: { Authorization: `Bearer ${session.token}` }
    });
    return response.json();
  }, invoiceId);

  expect(latestInvoice.status).toBe('PendingApproval');
  expect(latestInvoice.decidedBy).toBeNull();
  expect(latestInvoice.approvalHistory).toEqual([]);
});
