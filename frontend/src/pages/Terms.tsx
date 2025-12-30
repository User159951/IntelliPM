import { Link } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';

export default function Terms() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-3xl space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link to="/register">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-2xl font-bold text-foreground">Terms & Conditions</h1>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Terms and Conditions of Use</CardTitle>
            <CardDescription>Last updated: December 18, 2025</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4 prose prose-sm max-w-none">
            <section>
              <h2 className="text-lg font-semibold mb-2">1. Acceptance of Terms</h2>
              <p className="text-muted-foreground">
                By accessing and using IntelliPM, you accept and agree to be bound by the terms and provision of this agreement.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">2. Use License</h2>
              <p className="text-muted-foreground">
                Permission is granted to temporarily use IntelliPM for personal and commercial project management purposes. This is the grant of a license, not a transfer of title.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">3. User Account</h2>
              <p className="text-muted-foreground">
                You are responsible for maintaining the confidentiality of your account and password. You agree to accept responsibility for all activities that occur under your account.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">4. Data Privacy</h2>
              <p className="text-muted-foreground">
                Your use of IntelliPM is also governed by our Privacy Policy. Please review our Privacy Policy to understand our practices regarding the collection and use of your data.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">5. Restrictions</h2>
              <p className="text-muted-foreground">
                You may not:
              </p>
              <ul className="list-disc list-inside text-muted-foreground space-y-1 ml-4">
                <li>Modify or copy the materials</li>
                <li>Use the materials for any commercial purpose without explicit written permission</li>
                <li>Attempt to decompile or reverse engineer any software contained in IntelliPM</li>
                <li>Remove any copyright or other proprietary notations from the materials</li>
              </ul>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">6. Disclaimer</h2>
              <p className="text-muted-foreground">
                The materials on IntelliPM are provided on an 'as is' basis. IntelliPM makes no warranties, expressed or implied, and hereby disclaims and negates all other warranties including, without limitation, implied warranties or conditions of merchantability, fitness for a particular purpose, or non-infringement of intellectual property or other violation of rights.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">7. Limitations</h2>
              <p className="text-muted-foreground">
                In no event shall IntelliPM or its suppliers be liable for any damages (including, without limitation, damages for loss of data or profit, or due to business interruption) arising out of the use or inability to use IntelliPM.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold mb-2">8. Revisions</h2>
              <p className="text-muted-foreground">
                IntelliPM may revise these terms of service at any time without notice. By using this service you are agreeing to be bound by the then current version of these terms of service.
              </p>
            </section>

            <div className="pt-4 border-t">
              <Button asChild className="w-full sm:w-auto">
                <Link to="/register">Back to Registration</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
